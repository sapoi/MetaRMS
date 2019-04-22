using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using SharedLibrary.Services;
using SharedLibrary.Structures;
using SharedLibrary.StaticFiles;

namespace Core.Helpers
{
    /// <summary>
    /// CacheHelper is used by the Razor pages to get application descriptors and user rights from the cache.
    /// </summary>
    public class CacheHelper
    {
        /// <summary>
        /// List of keys currently present in the cache
        /// </summary>
        static List<string> cacheKeys = new List<string>();
        /// <summary>
        /// This method gets returns application descriptor based on application id in token.
        /// If no descriptor is found, null is returned.
        /// </summary>
        /// <param name="cache">Cache to get the descriptor from</param>
        /// <param name="accountService">Account service used for loading the descriptor if not present in the cache</param>
        /// <param name="token">Token for authentication on the server side</param>
        /// <returns>ApplicationDescriptor or null</returns>
        public static async Task<ApplicationDescriptor> GetApplicationDescriptorFromCacheAsync(IMemoryCache cache, IAccountService accountService, JWTToken token)
        {
            // Get application id from token
            TokenHelper tokenHelper = new TokenHelper(token);
            var applicationId = tokenHelper.GetApplicationId();
            if (applicationId == null)
            {
                Logger.LogToConsole($"Token {token.Value} does not contain application id.");
                return null;
            }
            // Get application descriptor from cache if already there
            var cacheKeyName = Constants.CacheDescriptorPrefix + applicationId;
            var applicationDescriptor = (ApplicationDescriptor)cache.Get(cacheKeyName);
            if (applicationDescriptor != null)
                return applicationDescriptor;
            // Otherwise load it from the server
            var response = await accountService.GetApplicationDescriptor(token);
            // If server did not respond with succes code, return null
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogToConsole($"Could not load application descriptor from server for user with token {token.Value}");
                return null;
            }
            // Set cache options - keep in cache for this time, reset time if accessed
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30));
            // Save data in cache
            applicationDescriptor = JsonConvert.DeserializeObject<ApplicationDescriptor>(await response.Content.ReadAsStringAsync());
            cache.Set(cacheKeyName, applicationDescriptor, cacheEntryOptions);
            // Remember the new cache key
            cacheKeys.Add(cacheKeyName);

            return applicationDescriptor;
        }
        /// <summary>
        /// This method gets returns user rights based on rights id in token.
        /// If no rights are found, null is returned.
        /// </summary>
        /// <param name="cache">Cache to get the rights from</param>
        /// <param name="accountService">Account service used for loading the rights if not present in the cache</param>
        /// <param name="token">Token for authentication on the server side</param>
        /// <returns>RightsModel or null</returns>
        public static async Task<RightsModel> GetRightsFromCacheAsync(IMemoryCache cache, IAccountService accountService, JWTToken token)
        {
            // Get application name and user id from token
            TokenHelper tokenHelper = new TokenHelper(token);
            var applicationId = tokenHelper.GetApplicationId();
            var userId = tokenHelper.GetUserId();
            if (applicationId == null || userId == null)
            {
                Logger.LogToConsole($"Token {token.Value} does not contain application id or user id.");
                return null;
            }
            // Get rights from cache if already there
            var cacheKeyName = Constants.CacheRightsPrefix + applicationId + '_' + userId;
            var rightsModel = (RightsModel)cache.Get(cacheKeyName);
            if (rightsModel != null)
                return rightsModel;
            // Otherwise load it from the server
            var response = await accountService.GetRightsModel(token);
            // If server did not respond with succes code, return null
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogToConsole($"Could not load user rights from server for user with token {token.Value}");
                return null;
            }
            // Set cache options - keep in cache for this time, reset time if accessed
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30));
            // Save data in cache
            rightsModel = JsonConvert.DeserializeObject<RightsModel>(await response.Content.ReadAsStringAsync());
            cache.Set(cacheKeyName, rightsModel, cacheEntryOptions);
            // Remember the new cache key
            cacheKeys.Add(cacheKeyName);

            return rightsModel;
        }
        /// <summary>
        /// This method removes all Constants.CacheRightsPrefix + application entries from cache.
        /// </summary>
        /// <param name="cache">Cache to delete from</param>
        /// <param name="applicationId">For which application to delete the rights</param>
        public static void RemoveRightsFromCache(IMemoryCache cache, long applicationId)
        {
            var newCacheKeys = new List<string>();
            foreach (var key in cacheKeys)
            {
                // Remove all Constants.CacheRightsPrefix entries for a given application id
                if(key.StartsWith(Constants.CacheRightsPrefix + applicationId))
                    cache.Remove(key);
                // Otherwise keep key in cache keys
                else
                    newCacheKeys.Add(key);
            }
            cacheKeys = newCacheKeys;
        }
    }
}