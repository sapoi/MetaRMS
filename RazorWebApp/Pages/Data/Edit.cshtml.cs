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

namespace RazorWebApp.Pages.Account
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

        [BindProperty]
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public DatasetDescriptor DatasetDescriptor { get; set; }
        public Dictionary<String, Object> Data { get; set; }
        //public Dictionary<String, Object> InputData { get; set; }
        [BindProperty]
        public List<string> Keys { get; set; }
        [BindProperty]
        public List<string> ValueList { get; set; }
        [BindProperty]
        public string DatasetName { get; set; }
        [BindProperty]
        public long DataId { get; set; }

        public async Task<IActionResult> OnGetAsync(string datasetName, long id)
        {
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
            ApplicationDescriptor = await CacheAccessHelper.GetApplicationDescriptorFromCacheAsync(_cache, _accountService, appName, token.Value);
            DatasetDescriptor = ApplicationDescriptor.Datasets.Where(d => d.Name == datasetName).FirstOrDefault();
            if (DatasetDescriptor == null)
            {
                Logger.Log(DateTime.Now, "dataset neexistuje");
                return RedirectToPage("/Account/Login");
            }
            DatasetName = datasetName;
            DataId = id;
            if (ModelState.IsValid)
            {
                var response = await _dataService.GetById(appName, datasetName, id, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                Data = JsonConvert.DeserializeObject<Dictionary<String, Object>>(stringResponse);
                Keys = new List<string>();
                //InputData = new Dictionary<string, object>();
                foreach (var key in DatasetDescriptor.Attributes)
                {
                    //InputData.Add(key.Name, new object());
                    Keys.Add(key.Name);
                }
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            Dictionary<String, Object> inputData = new Dictionary<string, object>();
            for (int i = 0; i < Keys.Count; i++)
            {
                inputData.Add(Keys[i], ValueList[i]);
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
            if (DatasetName == null)
            {
                Logger.Log(DateTime.Now, "neni aktivni dataset");
                return RedirectToPage("/Account/Login");
            }
            await _dataService.PatchById(appName, DatasetName, DataId, inputData, token.Value);
            return RedirectToPage("/Data/Get");
        }
        
        public async Task<IActionResult> OnPostDatasetSelectAsync(string datasetName)
        {
            return RedirectToPage("Get", "", new {datasetName = datasetName});
        }
    }
}
