using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using SharedLibrary.Services;
using SharedLibrary.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using SharedLibrary.Descriptors;
using RazorWebApp.Helpers;
using SharedLibrary.Enums;
using RazorWebApp.Structures;

namespace RazorWebApp.Pages.User
{
    public class GetModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;

        public GetModel(IUserService userService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this._userService = userService;
            this._accountService = accountService;
            this._cache = memoryCache;
        }

        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public List<UserModel> Data { get; set; }
        // logged user's rights to system dataset Users
        public RightsEnum UsersRights { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync(string message = null)
        {
            if (ModelState.IsValid)
            {
                var token = AccessHelper.ValidateAuthentication(this);
                // if token is not valid, return to login page
                if (token == null)
                    return RedirectToPage("/Account/Login");
                // get application descriptor
                ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(_cache, _accountService, token);
                if (ApplicationDescriptor == null)
                    return RedirectToPage("/Errors/ServerError");
                // get rights
                var rights = await AccessHelper.GetUserRights(_cache, _accountService, token);
                if (rights == null)
                    return RedirectToPage("/Errors/ServerError");

                MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);

                var userRights = AccessHelper.GetSystemRights(SystemDatasetsEnum.Users, rights);
                if (userRights == null)
                    return RedirectToPage("/Errors/ServerError");
                if (userRights == RightsEnum.None)
                    return RedirectToPage("/Errors/Unauthorized");
                UsersRights = (RightsEnum)userRights;

                if (message != null)
                    Message = message;

                var response = await _userService.GetAll(ApplicationDescriptor.LoginAppName, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                List<UserModel> data = JsonConvert.DeserializeObject<List<UserModel>>(stringResponse);
                Data = data;//JsonConvert.DeserializeObject<RightsModel>(stringResponse);
            }
            return Page();
        }
        public IActionResult OnPostUserEditAsync(string dataId)
        {
            return RedirectToPage("Edit", "", new { id = dataId });
        }
        public async Task<IActionResult> OnPostUserDeleteAsync(long dataId)
        {
            var token = AccessHelper.ValidateAuthentication(this);
            // if token is not valid, return to login page
            if (token == null)
                return RedirectToPage("/Account/Login");
            // get application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(_cache, _accountService, token);
            if (ApplicationDescriptor == null)
                return RedirectToPage("/Errors/ServerError");

            // get rights
            var rights = await AccessHelper.GetUserRights(_cache, _accountService, token);
            if (rights == null)
                return RedirectToPage("/Errors/ServerError");

            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);

            var response = await _userService.DeleteById(ApplicationDescriptor.LoginAppName, dataId, token.Value);
            string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            // remove " form beginning and end of message
            return await OnGetAsync(message.Substring(1, message.Length - 2));
        }

        public IActionResult OnPostUserCreateAsync()
        {
            return RedirectToPage("Create", "");
        }
    }
}
