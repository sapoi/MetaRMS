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

namespace RazorWebApp.Pages.Data
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
        //public List<string> Keys { get; set; }
        public List<DatasetDescriptor> ReadAuthorizedDatasets { get; set; }
        public RightsEnum ActiveDatasetRights { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }

        public async Task<IActionResult> OnGetAsync(string datasetName = null)
        {
            // only if ActiveDatasetDescriptor is valid, check for model validity
            if (ModelState.IsValid)
            {
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
                    this.Data = new List<Dictionary<string, object>>();
                    return Page();
                }
                var activeDatasetRights = AccessHelper.GetActiveDatasetRights(ActiveDatasetDescriptor, rights);
                if (activeDatasetRights == null)
                    return RedirectToPage("/Errors/ServerError");
                ActiveDatasetRights = (RightsEnum)activeDatasetRights;


                // getting real data
                var response = await _dataService.GetAll(ApplicationDescriptor.AppName, ActiveDatasetDescriptor.Name, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                Data = JsonConvert.DeserializeObject<List<Dictionary<String, Object>>>(stringResponse);
            }
            return Page();
        }
        public async Task<IActionResult> OnPostDatasetSelectAsync(string datasetName)
        {
            return await OnGetAsync(datasetName);
        }
        public async Task<IActionResult> OnPostDataEditAsync(string datasetName, string dataId)
        {
            return RedirectToPage("Edit", "", new { datasetName = datasetName, id = dataId });
        }
        public async Task<IActionResult> OnPostDataDeleteAsync(string datasetName, long dataId)
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
            // var rights = await AccessHelper.GetUserRights(_cache, _accountService, token);
            // if (rights == null)
            //     return RedirectToPage("/Errors/ServerError");
            // ActiveDatasetDescriptor = AccessHelper.GetActiveDatasetDescriptor(ApplicationDescriptor, rights, datasetName);
            // if (ActiveDatasetDescriptor == null)
            //     return RedirectToPage("/Errors/ServerError");
                
            await _dataService.DeleteById(ApplicationDescriptor.AppName, datasetName, dataId, token.Value);
            return await OnGetAsync(datasetName);
        }

        public async Task<IActionResult> OnPostDataCreateAsync(string datasetName)
        {
            return RedirectToPage("Create", "", new { datasetName = datasetName });
        }
    }
}
