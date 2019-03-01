using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RazorWebApp.Helpers;
using RazorWebApp.Structures;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Services;
using static SharedLibrary.Structures.JWTToken;

namespace RazorWebApp.Helpers
{
    // frontend
    class AccessHelper
    {
        // validate if token from PageModel model is valid
        /// <summary>
        /// This method validates if the page model contains correct token.
        /// </summary>
        /// <param name="model">Page model to get the token from</param>
        /// <returns>Valid token or null</returns>
        public static AccessToken GetTokenFromPageModel(PageModel model)
        {
            // Get session data from page model
            var sessionData = model.HttpContext.Session.GetString("sessionJWT");
            if (sessionData == null)
            {
                Logger.LogToConsole($"New request from address {model.Request.Host.Host} without token.");
                return null;
            }
            // Parse token from session data
            AccessToken token;
            try
            {
                token = JsonConvert.DeserializeObject<AccessToken>(sessionData);
            }
            catch
            {
                Logger.LogToConsole($"New request from address {model.Request.Host.Host} with cookie data {sessionData} without token.");
                return null;
            }
            // Get application id and user id from token
            TokenHelper tokenHelper = new TokenHelper(token);
            // Application id
            var applicationId = tokenHelper.GetApplicationId();
            // If token did not contain application id
            if (!applicationId.HasValue)
            {
                Logger.LogToConsole($"There was no application id in token {token.Value}.");
                return null;
            }
            token.ApplicationId = (long)applicationId;
            // User id
            var userId = tokenHelper.GetUserId();
            // If token did not contain user id
            if (!userId.HasValue)
            {
                Logger.LogToConsole($"There was no user id in token {token.Value}.");
                return null;
            }
            token.UserId = (long)userId;

            // Return valid token
            return token;
        }
        public static RightsEnum? GetRights(Dictionary<long, RightsEnum> rights, long datasetId)
        {
            if (rights.Keys.Contains(datasetId))
                return rights[datasetId];
            return null;
        }
        public static async Task<ApplicationDescriptor> GetApplicationDescriptor(IMemoryCache _cache, IAccountService _accountService, AccessToken token)
        {
            // get application descriptor from cache
            ApplicationDescriptor applicationDescriptor = await CacheAccessHelper.GetApplicationDescriptorFromCacheAsync(_cache, _accountService, token);
            // if no application descriptor was found in token, log info and redirect user to login page
            if (applicationDescriptor == null)
            {
                Logger.LogToConsole("nenalezen odpovidajici deskriptor aplikace");
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
                Logger.LogToConsole("nenalezena prava uzivatele");
                return null;
            }
            return rights.DataDictionary;
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
            menuData.ApplicationName = applicationDescriptor.ApplicationName;
            menuData.UsersDatasetName = applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name;
            menuData.ReadAuthorizedDatasets = AccessHelper.GetReadAuthorizedDatasets(applicationDescriptor, rights);
            return menuData;
        }
        public static DatasetDescriptor GetActiveDatasetDescriptor(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rights, string datasetName)
        {
            // if datasetName was specified
            if (datasetName != null)
            {
                return applicationDescriptor.Datasets.Where(d => d.Name == datasetName).FirstOrDefault();
            }
            // if dataset name was not specified, select first dataset with at least read right (>= 1)
            return AccessHelper.GetReadAuthorizedDatasets(applicationDescriptor, rights).FirstOrDefault();                                      
        }
        public static RightsEnum? GetActiveDatasetRights(DatasetDescriptor datasetDescriptor, Dictionary<long, RightsEnum> rights)
        {
            var rightsKeyValuePair = rights.Where(r => r.Key == datasetDescriptor.Id).FirstOrDefault();
            if (rightsKeyValuePair.Equals(default(KeyValuePair<long, RightsEnum>)))
            {
                Logger.LogToConsole("nenalezena prava na dataset");
                return null;
            }
            return rightsKeyValuePair.Value;
        }
        public static RightsEnum? GetSystemRights(SystemDatasetsEnum dataset, Dictionary<long, RightsEnum> rights)
        {
            var systemRights = rights.Where(r => r.Key == ((long)dataset)).FirstOrDefault();
            if (systemRights.Equals(default(KeyValuePair<long, RightsEnum>)))
            {
                Logger.LogToConsole("nenalezena prava na prava");
                return null;
            }
            return systemRights.Value;
        }
        // public static void Logout(PageModel model)
        // {
            
        // }
    }
}