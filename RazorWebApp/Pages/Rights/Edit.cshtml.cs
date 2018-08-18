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

        //[BindProperty]
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        //public DatasetDescriptor DatasetDescriptor { get; set; }
        public RightsModel Data { get; set; }
        //public Dictionary<String, Object> InputData { get; set; }
        [BindProperty]
        public List<long> DatasetsIds { get; set; }
        [BindProperty]
        public List<string> ValueList { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        // [BindProperty]
        // public string DatasetName { get; set; }
        [BindProperty]
        public long DataId { get; set; }
        [BindProperty]
        public string RightsName { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
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
        
            DataId = id;
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
                // ReadAuthorizedDatasets = AccessHelper.GetReadAuthorizedDatasets(ApplicationDescriptor, rights);
                // ActiveDatasetDescriptor = AccessHelper.GetActiveDatasetDescriptor(ApplicationDescriptor, rights, datasetName);
                // if (ActiveDatasetDescriptor == null)
                // {
                //     // dummy
                //     ActiveDatasetDescriptor = new DatasetDescriptor { Name = "", Id = 0, Attributes = new List<AttributeDescriptor>() };
                //     return Page();
                // }
                // DatasetName = datasetName;
                DataId = id;
                // AttributesNames = new List<string>();
                // foreach (var attribute in ActiveDatasetDescriptor.Attributes)
                // {
                //     AttributesNames.Add(attribute.Name);
                // }

                DatasetsIds = new List<long>();
                //InputData = new Dictionary<string, object>();
                foreach (var key in ApplicationDescriptor.Datasets)
                {
                    //InputData.Add(key.Name, new object());
                    DatasetsIds.Add(key.Id);
                }
                DatasetsIds.Add((long)SystemDatasetsEnum.Users);
                DatasetsIds.Add((long)SystemDatasetsEnum.Rights);






                var response = await _rightsService.GetById(ApplicationDescriptor.AppName, id, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                Data = JsonConvert.DeserializeObject<RightsModel>(stringResponse);
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
            // // get rights
            // var rights = await AccessHelper.GetUserRights(_cache, _accountService, token);
            // if (rights == null)
            //     return RedirectToPage("/Errors/ServerError");
            // ActiveDatasetDescriptor = AccessHelper.GetActiveDatasetDescriptor(ApplicationDescriptor, rights, DatasetName);
            // if (ActiveDatasetDescriptor == null)
            //     return RedirectToPage("/Errors/ServerError");

            // data prepare
            Dictionary<String, Object> inputData = new Dictionary<string, object>();
            //inputData.Add("Name", RightsName);
            inputData.Add(((long)SystemDatasetsEnum.Users).ToString(), ValueList[0]);
            inputData.Add(((long)SystemDatasetsEnum.Rights).ToString(), ValueList[1]);
            for (int i = 2; i < DatasetsIds.Count + 2; i++)
            {
                inputData.Add(DatasetsIds[i - 2].ToString(), ValueList[i]);
            }


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
            RightsModel patchedRightsModel = new RightsModel() { Name = RightsName, Data = JsonConvert.SerializeObject(inputData) };
            await _rightsService.PatchById(ApplicationDescriptor.AppName, DataId, patchedRightsModel, token.Value);
            // delete old rights from cache
            CacheAccessHelper.RemoveRightsFromCache(_cache, ApplicationDescriptor.AppName, DataId);
            return RedirectToPage("/Rights/Get");
        }
    }
}
