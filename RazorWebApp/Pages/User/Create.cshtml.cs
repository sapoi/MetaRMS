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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorWebApp.Pages.User
{
    public class CreateModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly IRightsService _rightsService;
        private IMemoryCache _cache;

        public CreateModel(IUserService userService, IAccountService accountService, IRightsService rightsService, IMemoryCache memoryCache)
        {
            this._userService = userService;
            this._accountService = accountService;
            this._rightsService = rightsService;
            this._cache = memoryCache;
        }

        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        [BindProperty]
        public List<string> ValueList { get; set; }
        public IEnumerable<SelectListItem> UserRightsList { get; set; }
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (ModelState.IsValid)
            {
                // get token if valid
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

                var response = await _rightsService.GetAll(ApplicationDescriptor.LoginAppName, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                List<RightsModel> data = JsonConvert.DeserializeObject<List<RightsModel>>(stringResponse);

                UserRightsList = data.Select(x => 
                                             new SelectListItem
                                             {
                                                 Value = x.Id.ToString(),
                                                 Text = x.Name
                                             });
                
                //TODO
                // foreach (var key in ApplicationDescriptor.Datasets)
                //     DatasetsIds.Add(key.Id);
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // validation
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
            
            // data prepare
            long newRightsId;
            if (ValueList.Count < 3 || !long.TryParse(ValueList[2], out newRightsId))
                return RedirectToPage("/Errors/ServerError");
            string newUsername = ValueList[0];
            string newPassword = ValueList[1];
            //TODO bude z enumu
            long newRights = newRightsId;
            Dictionary<String, Object> inputData = new Dictionary<string, object>();
            for (int i = 3; i < ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes.Count + 3; i++)
            {
                inputData.Add(ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes[i - 3].Name, ValueList[i]);
            }
            
            UserModel newUserModel = new UserModel() { Username = newUsername, 
                                                         Password = newPassword,
                                                         RightsId = newRightsId,
                                                         Data = JsonConvert.SerializeObject(inputData) };
            var response = await _userService.Create(ApplicationDescriptor.LoginAppName, newUserModel, token.Value);
            string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                Message = message.Substring(1, message.Length - 2);
                return Page();
            }

            return RedirectToPage("/User/Get", new {message = message.Substring(1, message.Length - 2)});
        }
    }
}
