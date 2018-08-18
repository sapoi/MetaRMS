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

namespace RazorWebApp.Pages.Data
{
    public class CreateModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;

        public CreateModel(IDataService dataService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this._dataService = dataService;
            this._accountService = accountService;
            this._cache = memoryCache;
        }

        //[BindProperty]
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public DatasetDescriptor ActiveDatasetDescriptor { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        public List<DatasetDescriptor> ReadAuthorizedDatasets { get; set; }
        [BindProperty]
        public List<string> AttributesNames { get; set; }
        [BindProperty]
        public List<string> ValueList { get; set; }
        [BindProperty]
        public string DatasetName { get; set; }

        public async Task<IActionResult> OnGetAsync(string datasetName)
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
                foreach (var attribute in ActiveDatasetDescriptor.Attributes)
                    AttributesNames.Add(attribute.Name);
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
            ActiveDatasetDescriptor = AccessHelper.GetActiveDatasetDescriptor(ApplicationDescriptor, rights, DatasetName);
            if (ActiveDatasetDescriptor == null)
                return RedirectToPage("/Errors/ServerError");

            // data prepare
            Dictionary<String, Object> inputData = new Dictionary<string, object>();
            for (int i = 0; i < AttributesNames.Count; i++)
                inputData.Add(AttributesNames[i], ValueList[i]);
            
            await _dataService.Create(ApplicationDescriptor.AppName, DatasetName, inputData, token.Value);
            return RedirectToPage("/Data/Get");
        }
        
        public async Task<IActionResult> OnPostDatasetSelectAsync(string datasetName)
        {
            return RedirectToPage("Get", "", new {datasetName = datasetName});
        }
    }
}
