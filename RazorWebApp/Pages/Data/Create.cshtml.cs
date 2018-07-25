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

namespace RazorWebApp.Pages.Account
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

        [BindProperty]
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public DatasetDescriptor DatasetDescriptor { get; set; }
        [BindProperty]
        public List<string> Keys { get; set; }
        [BindProperty]
        public List<string> ValueList { get; set; }
        [BindProperty]
        public string DatasetName { get; set; }

        public async Task<IActionResult> OnGetAsync(string datasetName)
        {
            var token = AuthorizationHelper.GetToken(this);
            if (token == null)
            {
                Console.WriteLine("neni token");
                return RedirectToPage("/Account/Login");
            }
            var appName = AuthorizationHelper.GetAppNameFromToken(token);
            if (appName == null)
            {
                Console.WriteLine("v tokenu nebyl claim co se jmenuje ApplicationName");
                return RedirectToPage("/Account/Login");
            }
            ApplicationDescriptor = await CacheAccessHelper.GetApplicationDescriptorFromCacheAsync(_cache, _accountService, appName, token.token);
            DatasetDescriptor = ApplicationDescriptor.Datasets.Where(d => d.Name == datasetName).FirstOrDefault();
            if (DatasetDescriptor == null)
            {
                Console.WriteLine("dataset neexistuje");
                return RedirectToPage("/Account/Login");
            }
            DatasetName = datasetName;
            if (ModelState.IsValid)
            {
                //var response = await _dataService.GetById(appName, datasetName, id, token.token);
                //TODO kontrolovat chyby v response
                //string stringResponse = await response.Content.ReadAsStringAsync();
                //Data = JsonConvert.DeserializeObject<Dictionary<String, Object>>(stringResponse);
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
            var token = AuthorizationHelper.GetToken(this);
            if (token == null)
            {
                Console.WriteLine("neni token");
                return RedirectToPage("/Account/Login");
            }
            var appName = AuthorizationHelper.GetAppNameFromToken(token);
            if (appName == null)
            {
                Console.WriteLine("v tokenu nebyl claim co se jmenuje ApplicationName");
                return RedirectToPage("/Account/Login");
            }
            if (DatasetName == null)
            {
                Console.WriteLine("neni aktivni dataset");
                return RedirectToPage("/Account/Login");
            }
            await _dataService.Create(appName, DatasetName, inputData, token.token);
            return RedirectToPage("/Data/Get");
        }
        
        public async Task<IActionResult> OnPostDatasetSelectAsync(string datasetName)
        {
            return RedirectToPage("Get", "", new {datasetName = datasetName});
        }
    }
}
