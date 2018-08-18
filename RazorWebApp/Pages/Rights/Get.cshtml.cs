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
    public class GetModel : PageModel
    {
        private readonly IRightsService _rightsService;
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;

        public GetModel(IRightsService rightsService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this._rightsService = rightsService;
            this._accountService = accountService;
            this._cache = memoryCache;
        }

        [BindProperty]
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        //public DatasetDescriptor ActiveDatasetDescriptor { get; set; }
        //////////////public List<Dictionary<string, object>> Data { get; set; }
        public List<RightsModel> Data { get; set; }
        //public List<string> Keys { get; set; }
        //public List<string> ReadAuthorizedDatasetsNames { get; set; }
        //public RightsEnum ActiveDatasetRights { get; set; }
        public RightsEnum RightsRights { get; set; }
        //public Dictionary<long, RightsEnum> NavbarRights { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // // get AccessToken from PageModel
            // var token = AuthorizationHelper.GetTokenFromPageModel(this);
            // // if there is no token, log info and redirect user to login page
            // if (token == null)
            // {
            //     Logger.Log(DateTime.Now, "neni token");
            //     return RedirectToPage("/Account/Login");
            // }
            // // get application name from token
            // TokenHelper tokenHelper = new TokenHelper(token);
            // var appName = tokenHelper.GetAppName();
            // // if no application name was found in token, log info and redirect user to login page
            // if (appName == null)
            // {
            //     Logger.Log(DateTime.Now, "v tokenu nebzl calim co se jmenuje ApplicationName");
            //     return RedirectToPage("/Account/Login");
            // }
            // // get user if from token
            // var userId = tokenHelper.GetUserId();
            // // if no user id was found in token, log info and redirect user to login page
            // if (userId == null)
            // {
            //     Logger.Log(DateTime.Now, "v tokenu nebzl calim co se jmenuje UserId");
            //     return RedirectToPage("/Account/Login");
            // }
            // get application descriptor from cache
            // this.ApplicationDescriptor = await CacheAccessHelper.GetApplicationDescriptorFromCacheAsync(_cache, _accountService, token);
            // // if no application descriptor was found in token, log info and redirect user to login page
            // if (this.ApplicationDescriptor == null)
            // {
            //     Logger.Log(DateTime.Now, "nenalezen odpovidajici deskriptor aplikace");
            //     return RedirectToPage("/Account/Login");
            // }
            // // get user rights from cache
            // var rights = await CacheAccessHelper.GetRightsFromCacheAsync(_cache, _accountService, token);
            // // var rightsIdDict = rights.Where(r => r.Key == "DBId").FirstOrDefault();
            // // if (rightsIdDict.Equals(default(KeyValuePair<string, object>)))
            // // {
            // //     Logger.Log(DateTime.Now, "rights neobsehuji DBId");
            // //     //TODO error
            // //     return RedirectToPage("/Account/Login");
            // // }
            // // long rightsId;
            // // if (!long.TryParse(rightsIdDict.Value.ToString(), out rightsId))
            // // {
            // //     Logger.Log(DateTime.Now, "rights DBId neobsahuje ve value long");
            // //     //TODO error
            // //     return RedirectToPage("/Account/Login");
            // // }

            // var rightsRights = rights.Where(r => r.Key == ((long)SystemDatasetsEnum.Rights)).FirstOrDefault();
            // if (rightsRights.Equals(default(KeyValuePair<long, RightsEnum>)))
            // {
            //     Logger.Log(DateTime.Now, "nenalezena prava na prava");
            //     return RedirectToPage("/Account/Login");
            // }
            // RightsRights = (RightsEnum)rightsRights.Value;
            // if (RightsRights <= RightsEnum.None)
            // {
            //     Logger.Log(DateTime.Now, $"uzivatel {userId} nema pravo cist tabulku rights");
            //     return RedirectToPage("/Errors/Unauthorized");
            // }
//TODO comments

            // NavbarRights = new Dictionary<long, RightsEnum>();
            // NavbarRights = rights.Where(r => r.Key == -1 || r.Key == -2).ToDictionary(pair => pair.Key, pair => pair.Value);

            // get dataset with rights at least Read
            
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
                // ReadAuthorizedDatasets = AccessHelper.GetReadAuthorizedDatasets(ApplicationDescriptor, rights);
                // ActiveDatasetDescriptor = AccessHelper.GetActiveDatasetDescriptor(ApplicationDescriptor, rights, datasetName);
                // if (ActiveDatasetDescriptor == null)
                // {
                //     // dummy
                //     ActiveDatasetDescriptor = new DatasetDescriptor { Name = "", Id = 0, Attributes = new List<AttributeDescriptor>() };
                //     this.Data = new List<Dictionary<string, object>>();
                //     return Page();
                // }
                var rightsRights = AccessHelper.GetSystemRights(SystemDatasetsEnum.Rights, rights);
                if (rightsRights == null)
                    return RedirectToPage("/Errors/ServerError");
                if (rightsRights == RightsEnum.None)
                    return RedirectToPage("/Errors/Unauthorized");
                RightsRights = (RightsEnum)rightsRights;







                var response = await _rightsService.GetAll(ApplicationDescriptor.AppName, token.Value);
                //TODO kontrolovat chyby v response
                string stringResponse = await response.Content.ReadAsStringAsync();
                List<RightsModel> data = JsonConvert.DeserializeObject<List<RightsModel>>(stringResponse);
                Data = data;//JsonConvert.DeserializeObject<RightsModel>(stringResponse);
            }
            return Page();
        }
        public async Task<IActionResult> OnPostRightsEditAsync(string dataId)
        {
            return RedirectToPage("Edit", "", new {id = dataId});
            //return RedirectToPage("/Data/Edit/", );
        }
        public async Task<IActionResult> OnPostRightsDeleteAsync(long dataId)
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
            //     Logger.Log(DateTime.Now, "v tokenu nebzl claim co se jmenuje ApplicationName");
            //     return RedirectToPage("/Account/Login");
            // }

            var token = AccessHelper.ValidateAuthentication(this);
                // if token is not valid, return to login page
                if (token == null)
                    return RedirectToPage("/Account/Login");
                // get application descriptor
                ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(_cache, _accountService, token);
                if (ApplicationDescriptor == null)
                    return RedirectToPage("/Errors/ServerError");

            await _rightsService.DeleteById(ApplicationDescriptor.AppName, dataId, token.Value);
            return await OnGetAsync();
        }

        public async Task<IActionResult> OnPostRightsCreateAsync()
        {
            return RedirectToPage("Create", "");
        }
    }
}
