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
        private readonly IDataService _dataService;

        public CreateModel(IUserService userService, IAccountService accountService, IRightsService rightsService, IMemoryCache memoryCache, IDataService dataService)
        {
            this._userService = userService;
            this._accountService = accountService;
            this._rightsService = rightsService;
            this._cache = memoryCache;
            this._dataService = dataService;
        }

        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        [BindProperty]
        public long NewRightsId { get; set; }
        [BindProperty]
        public Dictionary<string, List<string>> ValueList { get; set; }
        [BindProperty]
        public Dictionary<string, List<SelectListItem>> SelectData { get; set; }
        [BindProperty]
        public List<string> AttributesNames { get; set; }
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

                // init ValueList
                ValueList = new Dictionary<string, List<string>>();
                AttributesNames = new List<string>();
                foreach (var attribute in ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes)
                {
                    AttributesNames.Add(attribute.Name);
                    ValueList.Add(attribute.Name, new List<string>());
                }

                var response = await _rightsService.GetAll(ApplicationDescriptor.LoginApplicationName, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                List<RightsModel> data = JsonConvert.DeserializeObject<List<RightsModel>>(stringResponse);

                UserRightsList = data.Select(x => 
                                             new SelectListItem
                                             {
                                                 Value = x.Id.ToString(),
                                                 Text = x.Name
                                             });
                
                // fill SelectData
                DataLoadingHelper dlh = new DataLoadingHelper();
                SelectData = await dlh.FillSelectData(ApplicationDescriptor, 
                                                      ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes, 
                                                      _userService, 
                                                      _dataService, 
                                                      token);
                
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
            
            // // data prepare
            // long newRightsId;
            // if (ValueList.Count < 2 || !long.TryParse(ValueList[1][0], out newRightsId))
            //     return RedirectToPage("/Errors/ServerError");
            // string newUsername = ValueList[0][0];
            // // string newPassword = ValueList[1][0];
            // //TODO bude z enumu
            // long newRights = newRightsId;
            // Dictionary<String, Object> inputData = new Dictionary<string, object>();
            // for (int i = 2; i < ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes.Count + 2; i++)
            // {
            //     inputData.Add(ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes[i - 2].Name, ValueList[i]);
            // }

            // foreach (var attribute in ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes)
            // {
            //     // if nothing was selected in a select box, the key (attributeName) was removed and 
            //     // before sending new values to API, the key needs to be reentered with empty values
            //     if (!ValueList.Keys.Contains(attribute.Name))
            //         ValueList.Add(attribute.Name, new List<string>());
            // }

            //INPUT VALIDATIONS
            var validationHelper = new ValidationHelper();
            validationHelper.ValidateValueList(ValueList, ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes);
            
            UserModel newUserModel = new UserModel() { //Username = newUsername, 
                                                        //  Password = newPassword,
                                                         RightsId = NewRightsId,
                                                         Data = JsonConvert.SerializeObject(ValueList) };
            var response = await _userService.Create(ApplicationDescriptor.LoginApplicationName, newUserModel, token.Value);
            string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                // get rights
                var rights = await AccessHelper.GetUserRights(_cache, _accountService, token);
                if (rights == null)
                return RedirectToPage("/Errors/ServerError");
                // get menu data
                MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
                Message = message.Substring(1, message.Length - 2);

                // fill SelectData
                DataLoadingHelper dlh = new DataLoadingHelper();
                SelectData = await dlh.FillSelectData(ApplicationDescriptor, 
                                                      ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes, 
                                                      _userService, 
                                                      _dataService, 
                                                      token);

                return Page();
            }

            return RedirectToPage("/User/Get", new {message = message.Substring(1, message.Length - 2)});
        }
    }
}
