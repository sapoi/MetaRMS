using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Structures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Services;
using SharedLibrary.Structures;
using SharedLibrary.StaticFiles;

namespace Core.Helpers
{
    // frontend
    /// <summary>
    /// AccessHelper is used by the Razor pages. This helper contains methods neccessary to provide access to 
    /// individual parts of the application based on rights of the logged user and the application descriptor.
    /// </summary>
    class AccessHelper
    {
        /// <summary>
        /// This method validates if the page model contains correct token.
        /// </summary>
        /// <param name="model">Page model to get the token from</param>
        /// <returns>Valid token or null</returns>
        public static JWTToken GetTokenFromPageModel(PageModel model)
        {
            // Get session data from page model
            var sessionData = model.HttpContext.Session.GetString(Constants.SessionJWTKey);
            if (sessionData == null)
            {
                Logger.LogToConsole($"New request from address {model.Request.Host.Host} without token.");
                return null;
            }
            // Parse token from session data
            JWTToken token;
            try
            {
                token = JsonConvert.DeserializeObject<JWTToken>(sessionData);
            }
            catch
            {
                Logger.LogToConsole($"New request from address {model.Request.Host.Host} with cookie data {sessionData} without token.");
                return null;
            }
            // Get application id and user id from token
            TokenHelper tokenHelper = new TokenHelper(token);
            // Check if token not expired
            if (tokenHelper.IsExpired())
            {
                // And if token is expired, remove it from page model and return null
                model.HttpContext.Session.Remove(Constants.SessionJWTKey);
                return null;
            }
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
        /// <summary>
        /// This method gets returns application descriptor based on application id in token.
        /// If no descriptor is found, null is returned.
        /// </summary>
        /// <param name="cache">Cache to get the descriptor from</param>
        /// <param name="accountService">Account service used for loading the descriptor if not present in the cache</param>
        /// <param name="token">Token for authentication on the server side</param>
        /// <returns>Application descriptor or null</returns>
        public static async Task<ApplicationDescriptor> GetApplicationDescriptor(IMemoryCache cache, IAccountService accountService, JWTToken token)
        {
            // Get application descriptor from cache
            return await CacheHelper.GetApplicationDescriptorFromCacheAsync(cache, accountService, token);
        }
        /// <summary>
        /// This method gets returns user rights dictionary based on rights id in token.
        /// If no rights are found, null is returned.
        /// </summary>
        /// <param name="cache">Cache to get the rights from</param>
        /// <param name="accountService">Account service used for loading the rights if not present in the cache</param>
        /// <param name="token">Token for authentication on the server side</param>
        /// <returns>Rights dictionary or null</returns>
        public static async Task<Dictionary<long, RightsEnum>> GetUserRights(IMemoryCache cache, IAccountService accountService, JWTToken token)
        {
            // Get user rights from cache
            var rights = await CacheHelper.GetRightsFromCacheAsync(cache, accountService, token);
            // If no user rights were found, log info and return null
            if (rights == null)
                return null;
            // Otherwise return data dictionary from the rights found
            return rights.DataDictionary;
        }
        /// <summary>
        /// This method returns list of user-defined dataset descriptors that can be read by the user with rights from the parameter.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor containing all available datasets</param>
        /// <param name="rights">Rights dictionary with rights for each dataset from application descriptor</param>
        /// <returns>List of user-defined dataset descriptors with at least R rights</returns>
        public static List<DatasetDescriptor> GetReadAuthorizedUserDefinedDatasets(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rights)
        {
            // Get user-defined datasets with at least R rights
            var readAuthorizedDatasetsDict = rights.Where(r => r.Value >= RightsEnum.R && r.Key != (long)SystemDatasetsEnum.Users && r.Key != (long)SystemDatasetsEnum.Rights)
                                                   .ToDictionary(pair => pair.Key, pair => pair.Value); 
            // For every read authorized dataset get its descriptor
            var readAuthorizedDatasets = new List<DatasetDescriptor>();
            foreach (var dataset in applicationDescriptor.Datasets)
            {
                if (readAuthorizedDatasetsDict.ContainsKey(dataset.Id))
                    readAuthorizedDatasets.Add(dataset);
            }
            return readAuthorizedDatasets;
        }
        /// <summary>
        /// This method returns dictionary or rights for system datasets. At the moment rights for 
        /// Users and Rights datasets are returned.
        /// </summary>
        /// <param name="rights">Rights dictionary with rights for each dataset from application descriptor</param>
        /// <returns>Dictionary of rights for system datasets</returns>
        public static Dictionary<long, RightsEnum> GetSystemDatasetsRightsDict(Dictionary<long, RightsEnum> rights)
        {
            return rights.Where(r => r.Key == (long)SystemDatasetsEnum.Users || r.Key == (long)SystemDatasetsEnum.Rights)
                         .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        /// <summary>
        /// This method fills LoggedMenuPartialData structure with values based on application descriptor and
        /// rights provided in the parametres.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor to fill the structure from</param>
        /// <param name="rights">Rights dictionary of user that wants the LoggedMenuPartialData</param>
        /// <returns>Filled LoggedMenuPartialData structure</returns>
        public static LoggedMenuPartialData GetMenuData(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rights)
        {
            var menuData = new LoggedMenuPartialData();
            menuData.SystemDatasetsRights = AccessHelper.GetSystemDatasetsRightsDict(rights);
            menuData.ApplicationName = applicationDescriptor.ApplicationName;
            menuData.UsersDatasetName = applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name;
            menuData.ReadAuthorizedDatasets = AccessHelper.GetReadAuthorizedUserDefinedDatasets(applicationDescriptor, rights);
            return menuData;
        }
        /// <summary>
        /// This function returns dataset descriptor based on datasetName parameter.static If this parameter is not specified
        /// descriptor of first dataset with at least read rights is returned.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor to get the dataset descriptor from</param>
        /// <param name="rights">Rights of the user that wants the dataset</param>
        /// <param name="datasetName">Optional name of the dataset to get</param>
        /// <returns>Dataset descriptor based on its name or first with at least read rights</returns>
        public static DatasetDescriptor GetActiveDatasetDescriptor(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rights, string datasetName)
        {
            // If datasetName was specified
            if (datasetName != null)
                return applicationDescriptor.Datasets.Where(d => d.Name == datasetName).FirstOrDefault();
            // If dataset name was not specified, select first dataset with at least read right
            return AccessHelper.GetReadAuthorizedUserDefinedDatasets(applicationDescriptor, rights).FirstOrDefault();                                      
        }
    }
}