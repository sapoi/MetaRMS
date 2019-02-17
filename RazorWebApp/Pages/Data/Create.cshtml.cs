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
using RazorWebApp.Structures;
using SharedLibrary.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorWebApp.Pages.Data
{
    public class CreateModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;
        private readonly IUserService _userService;

        public CreateModel(IDataService dataService, IAccountService accountService, IMemoryCache memoryCache, IUserService userService)
        {
            this._dataService = dataService;
            this._accountService = accountService;
            this._cache = memoryCache;
            this._userService = userService;
        }

        //[BindProperty]
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public DatasetDescriptor ActiveDatasetDescriptor { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        [BindProperty]
        public Dictionary<string, List<SelectListItem>> SelectData { get; set; }
        public List<DatasetDescriptor> ReadAuthorizedDatasets { get; set; }
        [BindProperty]
        public List<string> AttributesNames { get; set; }
        [BindProperty]
        public Dictionary<string, List<string>> ValueList { get; set; }
        [BindProperty]
        public string DatasetName { get; set; }

        public async Task<IActionResult> OnGetAsync(string datasetName)
        {
            if (ModelState.IsValid)
            {
                // get token if valid
                var token = AccessHelper.GetTokenFromPageModel(this);
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
                ReadAuthorizedDatasets = AccessHelper.GetReadAuthorizedDatasets(ApplicationDescriptor, rights);
                ActiveDatasetDescriptor = AccessHelper.GetActiveDatasetDescriptor(ApplicationDescriptor, rights, datasetName);
                if (ActiveDatasetDescriptor == null)
                {
                    // dummy
                    ActiveDatasetDescriptor = new DatasetDescriptor { Name = "", Id = 0, Attributes = new List<AttributeDescriptor>() };
                    return Page();
                }
                DatasetName = datasetName;
                AttributesNames = new List<string>();
                // init ValueList
                ValueList = new Dictionary<string, List<string>>();
                foreach (var attribute in ActiveDatasetDescriptor.Attributes)
                {
                    AttributesNames.Add(attribute.Name);
                    ValueList.Add(attribute.Name, new List<string>());
                }

                // fill SelectData
                DataLoadingHelper dlh = new DataLoadingHelper();
                SelectData = await dlh.FillSelectData(ApplicationDescriptor, 
                                                      ActiveDatasetDescriptor.Attributes, 
                                                      _userService, 
                                                      _dataService, 
                                                      token);
                

            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // validation
            var token = AccessHelper.GetTokenFromPageModel(this);
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
            ActiveDatasetDescriptor = AccessHelper.GetActiveDatasetDescriptor(ApplicationDescriptor, rights, DatasetName);
            if (ActiveDatasetDescriptor == null)
                return RedirectToPage("/Errors/ServerError");

            // data prepare
            // Dictionary<String, Object> inputData = new Dictionary<string, object>();
            // foreach (var attributeName in AttributesNames)
            // {
            //     // if nothing was selected in a select box, the key (attributeName) was removed and 
            //     // before sending new values to API, the key needs to be reentered with empty values
            //     if (!ValueList.Keys.Contains(attributeName))
            //         ValueList.Add(attributeName, new List<string>());
            //     // if simple type field for attributeName is not filled in, ValueList[attributeName][0] == null
            //     // the database and backend does not accept null as value, so the whole ValueList[attributeName]
            //     // needs to be replaced by new List<string>
            //     if (ValueList[attributeName].Count == 1 && ValueList[attributeName][0] == null)
            //         ValueList[attributeName] = new List<string>();
            // }
            // for (int i = 0; i < AttributesNames.Count; i++)
            //     inputData.Add(AttributesNames[i], ValueList[AttributesNames[i]]);
            var validationHelper = new ValidationHelper();
            validationHelper.ValidateValueList(ValueList, ActiveDatasetDescriptor.Attributes);

            var newDataModel = new DataModel(){
                DatasetId = ActiveDatasetDescriptor.Id,
                Data = JsonConvert.SerializeObject(ValueList)
            };
            
            var response = await _dataService.Create(newDataModel, token.Value);
            string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return RedirectToPage("/Data/Get", new {message = message.Substring(1, message.Length - 2)});
        }
        
        public IActionResult OnPostDatasetSelectAsync(string datasetName)
        {
            return RedirectToPage("Get", "", new {datasetName = datasetName});
        }
    }
}
