using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using SharedLibrary.Services;
using static SharedLibrary.Structures.JWTToken;

namespace RazorWebApp.Helpers
{
    public class CacheAccessHelper
    {
        static List<string> cacheKeys = new List<string>();
        public static async Task<ApplicationDescriptor> GetApplicationDescriptorFromCacheAsync(IMemoryCache cache, IAccountService _accountService, AccessToken token)
        {
            TokenHelper tokenHelper = new TokenHelper(token);
            // get application name from token
            var applicationId = tokenHelper.GetApplicationId();
            // todo kontrolovat, ze neni null
            return await cache.GetOrCreateAsync("Descriptor_" + applicationId, async entry =>
              {
                  entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                  var response = await _accountService.GetApplicationDescriptor(token.Value);
                  var stringResponse = await response.Content.ReadAsStringAsync();
                  cacheKeys.Add("Descriptor_" + applicationId);
              return JsonConvert.DeserializeObject<ApplicationDescriptor>(stringResponse);
              });
        }
        public static async Task<RightsModel> GetRightsFromCacheAsync(IMemoryCache cache, IAccountService _accountService, AccessToken token)
        {
            TokenHelper tokenHelper = new TokenHelper(token);
            // get application name and user id from token
            var applicationId = tokenHelper.GetApplicationId();
            // todo kontrolovat, ze neni null
            var userId = tokenHelper.GetUserId();
            
            return await
                cache.GetOrCreateAsync("Rights_" + applicationId + '_' + userId, async entry =>
              {
                  entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                  var response = await _accountService.GetRights(token.Value);
                  var stringResponse = await response.Content.ReadAsStringAsync();
                  cacheKeys.Add("Rights_" + applicationId + '_' + userId);
                  return JsonConvert.DeserializeObject<RightsModel>(stringResponse);
              });
        }
        public static void RemoveRightsFromCache(IMemoryCache cache)
        {
            foreach (var key in cacheKeys)
            {
                if(key.StartsWith("Rights_"))
                    cache.Remove(key);
            }
        }
    }
}