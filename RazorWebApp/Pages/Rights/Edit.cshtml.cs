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

namespace RazorWebApp.Pages.Rights
{
    public class EditModel : PageModel
    {
        private readonly IRightsService _rightsService;
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;

        public EditModel(IRightsService rightsService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this._rightsService = rightsService;
            this._accountService = accountService;
            this._cache = memoryCache;
        }

        [BindProperty]
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        //public DatasetDescriptor DatasetDescriptor { get; set; }
        public RightsModel Data { get; set; }
        //public Dictionary<String, Object> InputData { get; set; }
        [BindProperty]
        public List<long> Keys { get; set; }
        [BindProperty]
        public List<string> ValueList { get; set; }
        // [BindProperty]
        // public string DatasetName { get; set; }
        [BindProperty]
        public long DataId { get; set; }
        [BindProperty]
        public string RightsName { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
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
        
            DataId = id;
            if (ModelState.IsValid)
            {
                var response = await _rightsService.GetById(appName, id, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                Data = JsonConvert.DeserializeObject<RightsModel>(stringResponse);
                Keys = new List<long>();
                //InputData = new Dictionary<string, object>();
                foreach (var key in ApplicationDescriptor.Datasets)
                {
                    //InputData.Add(key.Name, new object());
                    Keys.Add(key.Id);
                }
                Keys.Add((long)SystemDatasetsEnum.Users);
                Keys.Add((long)SystemDatasetsEnum.Rights);
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            Dictionary<String, Object> inputData = new Dictionary<string, object>();
            //inputData.Add("Name", RightsName);
            inputData.Add(((long)SystemDatasetsEnum.Users).ToString(), ValueList[0]);
            inputData.Add(((long)SystemDatasetsEnum.Rights).ToString(), ValueList[1]);
            for (int i = 2; i < Keys.Count + 2; i++)
            {
                inputData.Add(Keys[i - 2].ToString(), ValueList[i]);
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
            RightsModel patchedRightsModel = new RightsModel() { Name = RightsName, Data = JsonConvert.SerializeObject(inputData) };
            await _rightsService.PatchById(appName, DataId, patchedRightsModel, token.Value);
            // delete old rights from cache
            CacheAccessHelper.RemoveRightsFromCache(_cache, appName, DataId);
            return RedirectToPage("/Rights/Get");
        }
    }
}
