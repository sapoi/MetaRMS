using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using RazorWebApp.Structures;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Services;
using static SharedLibrary.Structures.JWTToken;

namespace RazorWebApp.Helpers
{
    class AccessHelper
    {
        // validate if token from PageModel model is valid
        public static AccessToken ValidateAuthentication(PageModel model)
        {
            // get AccessToken from PageModel
            AccessToken token = AuthorizationHelper.GetTokenFromPageModel(model);
            // if there is no token, log info and redirect user to login page
            if (token == null)
            {
                Logger.Log(DateTime.Now, "neni token");
                return null;
            }

            TokenHelper tokenHelper = new TokenHelper(token);
            // get application name from token
            var appName = tokenHelper.GetAppName();
            // if no application name was found in token, log info and redirect user to login page
            if (appName == null)
            {
                Logger.Log(DateTime.Now, "v tokenu nebyl claim co se jmenuje ApplicationName");
                return null;
            }
            // get user if from token
            var userId = tokenHelper.GetUserId();
            // if no user id was found in token, log info and redirect user to login page
            if (userId == null)
            {
                Logger.Log(DateTime.Now, "v tokenu nebyl claim co se jmenuje UserId");
                return null;
            }
            // token is valid
            return token;
        }
        public static async Task<ApplicationDescriptor> GetApplicationDescriptor(IMemoryCache _cache, IAccountService _accountService, AccessToken token)
        {
            // get application descriptor from cache
            ApplicationDescriptor applicationDescriptor = await CacheAccessHelper.GetApplicationDescriptorFromCacheAsync(_cache, _accountService, token);
            // if no application descriptor was found in token, log info and redirect user to login page
            if (applicationDescriptor == null)
            {
                Logger.Log(DateTime.Now, "nenalezen odpovidajici deskriptor aplikace");
                return null;
            }
            return applicationDescriptor;
        }
        public static async Task<Dictionary<long, RightsEnum>> GetUserRights(IMemoryCache _cache, IAccountService _accountService, AccessToken token)
        {
            // get user rights from cache
            var rights = await CacheAccessHelper.GetRightsFromCacheAsync(_cache, _accountService, token);
            if (rights == null)
            {
                Logger.Log(DateTime.Now, "nenalezena prava uzivatele");
                return null;
            }
            return rights;
        }
        public static List<DatasetDescriptor> GetReadAuthorizedDatasets(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rights)
        {
            // get dataset with rights at least Read
            var readAuthorizedDatasetsDict = rights.Where(r => r.Value >= RightsEnum.R && r.Key != -1 && r.Key != -2)
                                                   .ToDictionary(pair => pair.Key, pair => pair.Value); 
            var readAuthorizedDatasets = new List<DatasetDescriptor>();
            foreach (var dataset in applicationDescriptor.Datasets)
            {
                if (readAuthorizedDatasetsDict.ContainsKey(dataset.Id))
                    readAuthorizedDatasets.Add(dataset);
            }
            return readAuthorizedDatasets;
        }
        public static Dictionary<long, RightsEnum> GetNavbarRightsDict(Dictionary<long, RightsEnum> rights)
        {
            return rights.Where(r => r.Key == -1 || r.Key == -2).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        public static LoggedMenuPartialData GetMenuData(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rights)
        {
            var menuData = new LoggedMenuPartialData();
            menuData.NavbarRights = AccessHelper.GetNavbarRightsDict(rights);
            menuData.AppName = applicationDescriptor.AppName;
            return menuData;
        }
        public static DatasetDescriptor GetActiveDatasetDescriptor(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rights, string datasetName)
        {
            DatasetDescriptor activeDatasetDescriptor;
            var readAuthorizedDatasets = AccessHelper.GetReadAuthorizedDatasets(applicationDescriptor, rights);                                      
            if (readAuthorizedDatasets.Count() == 0)
            {
                Logger.Log(DateTime.Now, "uzivatel nema zadne read authorized descriptory");
                return null;
            }
            // if no dataset was specified in parameter, select first dataset with at least read right (>= 1)
            if (datasetName == null)
                return readAuthorizedDatasets[0];
            activeDatasetDescriptor = readAuthorizedDatasets.Where(d => d.Name == datasetName).FirstOrDefault();
            if (activeDatasetDescriptor == null)
                return readAuthorizedDatasets[0];
            return activeDatasetDescriptor;
        }
        public static RightsEnum? GetActiveDatasetRights(DatasetDescriptor datasetDescriptor, Dictionary<long, RightsEnum> rights)
        {
            var rightsKeyValuePair = rights.Where(r => r.Key == datasetDescriptor.Id).FirstOrDefault();
            if (rightsKeyValuePair.Equals(default(KeyValuePair<long, RightsEnum>)))
            {
                Logger.Log(DateTime.Now, "nenalezena prava na dataset");
                return null;
            }
            return rightsKeyValuePair.Value;
        }
        public static RightsEnum? GetSystemRights(SystemDatasetsEnum dataset, Dictionary<long, RightsEnum> rights)
        {
            var systemRights = rights.Where(r => r.Key == ((long)dataset)).FirstOrDefault();
            if (systemRights.Equals(default(KeyValuePair<long, RightsEnum>)))
            {
                Logger.Log(DateTime.Now, "nenalezena prava na prava");
                return null;
            }
            return systemRights.Value;
        }
        public static void Logout(PageModel model)
        {
            
        }
    }
}