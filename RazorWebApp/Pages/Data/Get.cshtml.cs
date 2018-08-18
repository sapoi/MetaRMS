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
        //public Dictionary<long, RightsEnum> NavbarRights { get; set; }

        public async Task<IActionResult> OnGetAsync(string datasetName = null)
        {
            //             // get AccessToken from PageModel
            //             var token = AuthorizationHelper.GetTokenFromPageModel(this);
            //             // if there is no token, log info and redirect user to login page
            //             if (token == null)
            //             {
            //                 Logger.Log(DateTime.Now, "neni token");
            //                 return RedirectToPage("/Account/Login");
            //             }
            //             // get application name from token
            //             TokenHelper tokenHelper = new TokenHelper(token);
            //             var appName = tokenHelper.GetAppName();
            //             // if no application name was found in token, log info and redirect user to login page
            //             if (appName == null)
            //             {
            //                 Logger.Log(DateTime.Now, "v tokenu nebzl calim co se jmenuje ApplicationName");
            //                 return RedirectToPage("/Account/Login");
            //             }
            //             // get user if from token
            //             var userId = tokenHelper.GetUserId();
            //             // if no user id was found in token, log info and redirect user to login page
            //             if (userId == null)
            //             {
            //                 Logger.Log(DateTime.Now, "v tokenu nebzl calim co se jmenuje UserId");
            //                 return RedirectToPage("/Account/Login");
            //             }
            //             // get application descriptor from cache
            //             this.ApplicationDescriptor = await CacheAccessHelper.GetApplicationDescriptorFromCacheAsync(_cache, _accountService, appName, token.Value);
            //             // if no application descriptor was found in token, log info and redirect user to login page
            //             if (this.ApplicationDescriptor == null)
            //             {
            //                 Logger.Log(DateTime.Now, "nenalezen odpovidajici deskriptor aplikace");
            //                 return RedirectToPage("/Account/Login");
            //             }

            // get token if valid



            // var activeDatasetRights = readAuthorizedDatasets.Where(d => d.Key == ActiveDatasetDescriptor.Id).FirstOrDefault();
            // if (!activeDatasetRights.Equals(default(KeyValuePair<long, RightsEnum>)))
            //     ActiveDatasetRights = activeDatasetRights.Value;





            //             // get user rights from cache
            //             var rights = await CacheAccessHelper.GetRightsFromCacheAsync(_cache, _accountService, appName, (long)userId, token.Value);


            // //TODO comments
            //             //TODO chytat vyjimky
            //             MenuData = new LoggedMenuPartialData();
            //             MenuData.NavbarRights = new Dictionary<long, RightsEnum>();
            //             MenuData.NavbarRights = rights.Where(r => r.Key == -1 || r.Key == -2).ToDictionary(pair => pair.Key, pair => pair.Value);
            //             MenuData.AppName = appName;
            // get dataset with rights at least Read
            // var readAuthorizedDatasets = rights.Where(r => r.Value >= RightsEnum.R && r.Key != -1 && r.Key != -2)
            //                                    .ToDictionary(pair => pair.Key, pair => pair.Value);                                           

            // // and if there are any
            // if (readAuthorizedDatasets.Count() != 0)
            // {
            //     DatasetDescriptor dataset;
            //     // if no dataset was specified in parameter, select first dataset with at least read right (>= 1)
            //     if (datasetName == null)
            //     {
            //         dataset = ApplicationDescriptor.Datasets.Where(d => d.Id == readAuthorizedDatasets.Keys.FirstOrDefault()).FirstOrDefault();
            //         if (dataset != null)
            //         {
            //             datasetName = dataset.Name;
            //         }
            //     }
            //     dataset = ApplicationDescriptor.Datasets.Where(d => d.Name == datasetName).FirstOrDefault();
            //     if (dataset != null)
            //         if (readAuthorizedDatasets.ContainsKey(dataset.Id))
            //             ActiveDatasetDescriptor = ApplicationDescriptor.Datasets.Where(d => d.Name == datasetName).FirstOrDefault();
            // }
            // ReadAuthorizedDatasetsNames = (from dataset in ApplicationDescriptor.Datasets
            //                               where readAuthorizedDatasets.ContainsKey(dataset.Id)
            //                               select dataset.Name).ToList();
            // //ReadAuthorizedDatasetsNames = from ApplicationDescriptor.Datasets.Where(d.Name => readAuthorizedDatasetsDict.ContainsKey(d.Id)).ToList();


            // // user doesnt have at least read rights to any dataset, return empty page
            // if (ActiveDatasetDescriptor == null)
            // {
            //     // dummy dataset
            //     ActiveDatasetDescriptor = new DatasetDescriptor { Name = "", Id = 0, Attributes = new List<AttributeDescriptor>() };
            //     this.Data = new List<Dictionary<string, object>>();
            //     //this.Keys = new List<string>();
            //     return Page();
            //     //TODO: zobrazit informaci pro uzivatele
            // }
            // // Keys = new List<string>();
            // // foreach (var key in ActiveDatasetDescriptor.Attributes)
            // // {
            // //     Keys.Add(key.Name);
            // // }
            // var activeDatasetRights = readAuthorizedDatasets.Where(d => d.Key == ActiveDatasetDescriptor.Id).FirstOrDefault();
            // if (!activeDatasetRights.Equals(default(KeyValuePair<long, RightsEnum>)))
            //     ActiveDatasetRights = activeDatasetRights.Value;







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
                ActiveDatasetRights = AccessHelper.GetActiveDatasetRights(ActiveDatasetDescriptor, rights);


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
            //return RedirectToPage("/Data/Edit/", );
        }
        public async Task<IActionResult> OnPostDataDeleteAsync(string datasetName, long dataId)
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
                Logger.Log(DateTime.Now, "v tokenu nebzl claim co se jmenuje ApplicationName");
                return RedirectToPage("/Account/Login");
            }
            if (datasetName == null)
            {
                Logger.Log(DateTime.Now, "neni aktivni dataset");
                return RedirectToPage("/Account/Login");
            }

            await _dataService.DeleteById(appName, datasetName, dataId, token.Value);
            return await OnGetAsync(datasetName);
        }

        public async Task<IActionResult> OnPostDataCreateAsync(string datasetName)
        {
            return RedirectToPage("Create", "", new { datasetName = datasetName });
        }
    }
}
