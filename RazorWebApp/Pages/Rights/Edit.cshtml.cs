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

        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public RightsModel Data { get; set; }
        [BindProperty]
        public List<long> DatasetsIds { get; set; }
        [BindProperty]
        public Dictionary<string, int> ValueList { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        [BindProperty]
        public long DataId { get; set; }
        [BindProperty]
        public string RightsName { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
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
                DataId = id;

                DatasetsIds = new List<long>();
                foreach (var key in ApplicationDescriptor.Datasets)
                    DatasetsIds.Add(key.Id);
                DatasetsIds.Add((long)SystemDatasetsEnum.Users);
                DatasetsIds.Add((long)SystemDatasetsEnum.Rights);

                var response = await _rightsService.GetById(ApplicationDescriptor.LoginApplicationName, id, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                Data = JsonConvert.DeserializeObject<RightsModel>(stringResponse);
                // init ValueList
                ValueList = Data.DataDictionary;
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
            // Dictionary<String, Object> inputData = new Dictionary<string, object>();
            // inputData.Add(((long)SystemDatasetsEnum.Users).ToString(), ValueList[0]);
            // inputData.Add(((long)SystemDatasetsEnum.Rights).ToString(), ValueList[1]);
            // for (int i = 2; i < DatasetsIds.Count + 2; i++)
            // {
            //     inputData.Add(DatasetsIds[i - 2].ToString(), ValueList[i]);
            // }
    
            RightsModel patchedRightsModel = new RightsModel() { Name = RightsName, Data = JsonConvert.SerializeObject(ValueList) };
            var response = await _rightsService.PatchById(ApplicationDescriptor.LoginApplicationName, DataId, patchedRightsModel, token.Value);
            string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            // delete old rights from cache
            CacheAccessHelper.RemoveRightsFromCache(_cache, ApplicationDescriptor.LoginApplicationName, DataId);
            return RedirectToPage("/Rights/Get",  new {message = message.Substring(1, message.Length - 2)});
        }
    }
}
