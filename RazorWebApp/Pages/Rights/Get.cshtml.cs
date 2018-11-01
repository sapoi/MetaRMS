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

namespace RazorWebApp.Pages.Rights
{
    public class GetModel : PageModel
    {
        private readonly IRightsService _rightsService;
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;

        public GetModel(IRightsService rightsService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this._rightsService = rightsService;
            this._accountService = accountService;
            this._cache = memoryCache;
        }

        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public List<RightsModel> Data { get; set; }
        public RightsEnum RightsRights { get; set; }
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

                var rightsRights = AccessHelper.GetSystemRights(SystemDatasetsEnum.Rights, rights);
                if (rightsRights == null)
                    return RedirectToPage("/Errors/ServerError");
                if (rightsRights == RightsEnum.None)
                    return RedirectToPage("/Errors/Unauthorized");
                RightsRights = (RightsEnum)rightsRights;

                if (message != null)
                    Message = message;

                var response = await _rightsService.GetAll(ApplicationDescriptor.LoginAppName, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                List<RightsModel> data = JsonConvert.DeserializeObject<List<RightsModel>>(stringResponse);
                Data = data;//JsonConvert.DeserializeObject<RightsModel>(stringResponse);
            }
            return Page();
        }
        public IActionResult OnPostRightsEditAsync(string dataId)
        {
            return RedirectToPage("Edit", "", new { id = dataId });
        }
        public async Task<IActionResult> OnPostRightsDeleteAsync(long dataId)
        {
            var token = AccessHelper.ValidateAuthentication(this);
            // if token is not valid, return to login page
            if (token == null)
                return RedirectToPage("/Account/Login");
            // get application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(_cache, _accountService, token);
            if (ApplicationDescriptor == null)
                return RedirectToPage("/Errors/ServerError");

            var response = await _rightsService.DeleteById(ApplicationDescriptor.LoginAppName, dataId, token.Value);
            string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            // remove " form beginning and end of message
            return await OnGetAsync(message.Substring(1, message.Length - 2));
        }

        public IActionResult OnPostRightsCreateAsync()
        {
            return RedirectToPage("Create", "");
        }
    }
}
