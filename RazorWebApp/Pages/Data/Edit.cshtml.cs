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
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public DatasetDescriptor ActiveDatasetDescriptor { get; set; }
        public List<DatasetDescriptor> ReadAuthorizedDatasets { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        public Dictionary<String, List<Object>> Data { get; set; }
        [BindProperty]
        public List<string> AttributesNames { get; set; }
        [BindProperty]
        public List<List<string>> ValueList { get; set; }
        [BindProperty]
        public string DatasetName { get; set; }
        [BindProperty]
        public long DataId { get; set; }
        [BindProperty]
        public Dictionary<string, List<SelectListItem>> SelectData { get; set; }
        public async Task<IActionResult> OnGetAsync(string datasetName, long id)
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
                DataId = id;
                AttributesNames = new List<string>();
                foreach (var attribute in ActiveDatasetDescriptor.Attributes)
                {
                    AttributesNames.Add(attribute.Name);
                }

                // fill SelectData
                SelectData = new Dictionary<string, List<SelectListItem>>();
                foreach (var attribute in ActiveDatasetDescriptor.Attributes)
                {
                    if (attribute.Type != "color" && attribute.Type != "date" && attribute.Type != "datetime" && 
                        attribute.Type != "email" && attribute.Type != "month" && attribute.Type != "int" && 
                        attribute.Type != "float" && attribute.Type != "year" && attribute.Type != "tel" && 
                        attribute.Type != "string" && attribute.Type != "time" && attribute.Type != "url" &&
                        attribute.Type != "bool" && attribute.Type != "text")
                        if (!SelectData.ContainsKey(attribute.Type))
                        {
                            // getting real data
                            var selectResponse = await _dataService.GetAll(ApplicationDescriptor.LoginAppName, attribute.Type, token.Value);
                            //TODO kontrolovat chyby v response
                            string selectStringResponse = await selectResponse.Content.ReadAsStringAsync();
                            var data = JsonConvert.DeserializeObject<List<Dictionary<String, Object>>>(selectStringResponse);
                            SelectData[attribute.Type] = data.Select(x => new SelectListItem { Value = JsonConvert.DeserializeObject<List<string>>
                                                                                                            (x["DBId"].ToString()).First(), 
                                                                                               Text =  JsonConvert.DeserializeObject<List<string>>
                                                                                                            (x[ApplicationDescriptor.Datasets.Where(d => d.Name == attribute.Type)
                                                                                                                                      .First()
                                                                                                                             .Attributes[0].Name
                                                                                                                ].ToString()).First() 
                                                                                             } 
                                                                    )
                                                             .ToList();
                        }
                }

                // getting real data
                var response = await _dataService.GetById(ApplicationDescriptor.LoginAppName, datasetName, id, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                Data = JsonConvert.DeserializeObject<Dictionary<String, List<Object>>>(stringResponse);
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

            var response = await _dataService.PatchById(ApplicationDescriptor.LoginAppName, DatasetName, DataId, inputData, token.Value);
            string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return RedirectToPage("/Data/Get",  new {message = message.Substring(1, message.Length - 2)});
        }

        public async Task<IActionResult> OnPostDatasetSelectAsync(string datasetName)
        {
            return RedirectToPage("Get", "", new { datasetName = datasetName });
        }
    }
}
