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

namespace RazorWebApp.Pages.Data
{
    public class EditModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;

        public EditModel(IDataService dataService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this._dataService = dataService;
            this._accountService = accountService;
            this._cache = memoryCache;
        }

        //[BindProperty]
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        //[BindProperty]
        public DatasetDescriptor ActiveDatasetDescriptor { get; set; }
        public Dictionary<String, Object> Data { get; set; }
        //public Dictionary<String, Object> InputData { get; set; }
        [BindProperty]
        public List<string> AttributesNames { get; set; }
        [BindProperty]
        public List<string> ValueList { get; set; }
        [BindProperty]
        public string DatasetName { get; set; }
        [BindProperty]
        public long DataId { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        public List<DatasetDescriptor> ReadAuthorizedDatasets { get; set; }
        //public RightsEnum ActiveDatasetRights { get; set; }

        public async Task<IActionResult> OnGetAsync(string datasetName, long id)
        {
            // var token = AuthorizationHelper.GetTokenFromPageModel(this);
            // if (token == null)
            // {
            //     Logger.Log(DateTime.Now, "neni token");
            //     return RedirectToPage("/Account/Login");
            // }
            // TokenHelper tokenHelper = new TokenHelper(token);
            // var appName = tokenHelper.GetAppName();
            // if (appName == null)
            // {
            //     Logger.Log(DateTime.Now, "v tokenu nebyl claim co se jmenuje ApplicationName");
            //     return RedirectToPage("/Account/Login");
            // }
            // ApplicationDescriptor = await CacheAccessHelper.GetApplicationDescriptorFromCacheAsync(_cache, _accountService, token);
            // DatasetDescriptor = ApplicationDescriptor.Datasets.Where(d => d.Name == datasetName).FirstOrDefault();
            // if (DatasetDescriptor == null)
            // {
            //     Logger.Log(DateTime.Now, "dataset neexistuje");
            //     return RedirectToPage("/Account/Login");
            // }
            // DatasetName = datasetName;
            // DataId = id;

            // var rights = await CacheAccessHelper.GetRightsFromCacheAsync(_cache, _accountService, token);

            // // MenuData = new LoggedMenuPartialData();
            // // MenuData.NavbarRights = new Dictionary<long, RightsEnum>();
            // // MenuData.NavbarRights = rights.Where(r => r.Key == -1 || r.Key == -2).ToDictionary(pair => pair.Key, pair => pair.Value);
            // // MenuData.AppName = appName;





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
                    //this.Data = new List<Dictionary<string, object>>();
                    return Page();
                }
                //ActiveDatasetRights = AccessHelper.GetActiveDatasetRights(ActiveDatasetDescriptor, rights);
                DatasetName = datasetName;
                DataId = id;
                AttributesNames = new List<string>();
                foreach (var attribute in ActiveDatasetDescriptor.Attributes)
                {
                    AttributesNames.Add(attribute.Name);
                }


                // getting real data
                var response = await _dataService.GetById(ApplicationDescriptor.AppName, datasetName, id, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                Data = JsonConvert.DeserializeObject<Dictionary<String, Object>>(stringResponse);
                // Keys = new List<string>();
                //InputData = new Dictionary<string, object>();
                // foreach (var key in DatasetDescriptor.Attributes)
                // {
                //     //InputData.Add(key.Name, new object());
                //     Keys.Add(key.Name);
                // }
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            Dictionary<String, Object> inputData = new Dictionary<string, object>();
            for (int i = 0; i < AttributesNames.Count; i++)
            {
                inputData.Add(AttributesNames[i], ValueList[i]);
            }
            var token = AuthorizationHelper.GetTokenFromPageModel(this);
            if (token == null)
            {
                Logger.Log(DateTime.Now, "neni token");
                return RedirectToPage("/Account/Login");
            }
            TokenHelper tokenHelper = new TokenHelper(token);
            var appName = tokenHelper.GetAppName();
            if (appName == null)
            {
                Logger.Log(DateTime.Now, "v tokenu nebyl claim co se jmenuje ApplicationName");
                return RedirectToPage("/Account/Login");
            }
            // if (ActiveDatasetDescriptor.Name == null)
            // {
            //     Logger.Log(DateTime.Now, "neni aktivni dataset");
            //     return RedirectToPage("/Account/Login");
            // }
            await _dataService.PatchById(appName, DatasetName, DataId, inputData, token.Value);
            return RedirectToPage("/Data/Get");
        }

        public async Task<IActionResult> OnPostDatasetSelectAsync(string datasetName)
        {
            return RedirectToPage("Get", "", new { datasetName = datasetName });
        }
    }
}
