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
    public class GetModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;

        public GetModel(IDataService dataService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this._dataService = dataService;
            this._accountService = accountService;
            this._cache = memoryCache;
        }

        [BindProperty]
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public DatasetDescriptor ActiveDatasetDescriptor { get; set; }
        public List<Dictionary<String, Object>> Data { get; set; }
        public List<long> Ids { get; set; }
        public List<string> Keys { get; set; }

        public async Task<IActionResult> OnGetAsync(string datasetName = null)
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
                Console.WriteLine("v tokenu nebzl calim co se jmenuje ApplicationName");
                return RedirectToPage("/Account/Login");
            }

            ApplicationDescriptor = await CacheAccessHelper.GetApplicationDescriptorFromCacheAsync(_cache, _accountService, appName, token.token);
            if (datasetName == null)
            {
                datasetName = ApplicationDescriptor.Datasets[0].Name;
            }
            ActiveDatasetDescriptor = ApplicationDescriptor.Datasets.Where(d => d.Name == datasetName).FirstOrDefault();
            if (ActiveDatasetDescriptor == null)
                ActiveDatasetDescriptor = ApplicationDescriptor.Datasets[0];
            if (ModelState.IsValid)
            {
                var response = await _dataService.GetAll(appName, datasetName, token.token);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                //TODO pridat do Data i id
                Data = JsonConvert.DeserializeObject<List<Dictionary<String, Object>>>(stringResponse);
                Keys = new List<string>();
                foreach (var key in ActiveDatasetDescriptor.Attributes)
                {
                    Keys.Add(key.Name);
                }
            }
            return Page();
        }
        public async Task<IActionResult> OnPostDatasetSelectAsync(string datasetName)
        {
            return await OnGetAsync(datasetName);
        }
        public async Task<IActionResult> OnPostDataEditAsync(string dataId)
        {
            return null;//await OnGetAsync(datasetName);
        }
        public async Task<IActionResult> OnPostDataDeleteAsync(string datasetName, long dataId)
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
                Console.WriteLine("v tokenu nebzl claim co se jmenuje ApplicationName");
                return RedirectToPage("/Account/Login");
            }
            if (datasetName == null)
            {
                Console.WriteLine("neni aktivni dataset");
                return RedirectToPage("/Account/Login");
            }

            await _dataService.DeleteById(appName, datasetName, dataId, token.token);
            return await OnGetAsync(datasetName);
        }
    }
}
