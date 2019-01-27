using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using SharedLibrary.Services;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using SharedLibrary.Descriptors;
using RazorWebApp.Helpers;
using SharedLibrary.Enums;
using RazorWebApp.Structures;
using SharedLibrary.Models;

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
        public List<DataModel> Data { get; set; }
        public List<DatasetDescriptor> ReadAuthorizedDatasets { get; set; }
        public RightsEnum ActiveDatasetRights { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync(string datasetName = null, string message = null)
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
                    this.Data = new List<DataModel>();
                    return Page();
                }
                var activeDatasetRights = AccessHelper.GetActiveDatasetRights(ActiveDatasetDescriptor, rights);
                if (activeDatasetRights == null)
                    return RedirectToPage("/Errors/ServerError");
                ActiveDatasetRights = (RightsEnum)activeDatasetRights;

                if (message != null)
                    Message = message;

                // getting real data
                var response = await _dataService.GetAll(ApplicationDescriptor.LoginAppName, ActiveDatasetDescriptor.Name, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                Data = JsonConvert.DeserializeObject<List<DataModel>>(stringResponse);
            }
            return Page();
        }
        public IActionResult OnPostDatasetSelectAsync(string datasetName)
        {
            return RedirectToPage("Get", "", new { datasetName = datasetName});
        }
        public IActionResult OnPostDataEditAsync(string datasetName, string dataId)
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

            var response = await _dataService.DeleteById(ApplicationDescriptor.LoginAppName, datasetName, dataId, token.Value);
            string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            // remove " form beginning and end of message
            return await OnGetAsync(datasetName, message.Substring(1, message.Length - 2));
        }

        public IActionResult OnPostDataCreateAsync(string datasetName)
        {
            return RedirectToPage("Create", "", new { datasetName = datasetName });
        }
    }
}
