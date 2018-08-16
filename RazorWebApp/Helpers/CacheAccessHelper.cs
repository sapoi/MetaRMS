using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using SharedLibrary.Services;

namespace RazorWebApp.Helpers
{
    public class CacheAccessHelper
    {
        static List<string> cacheKeys = new List<string>();
        public static async Task<ApplicationDescriptor> GetApplicationDescriptorFromCacheAsync(IMemoryCache cache, IAccountService _accountService, string appName, string token)
        {
            return await cache.GetOrCreateAsync("Descriptor_" + appName, async entry =>
              {
                  entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                  var response = await _accountService.GetApplicationDescriptorByAppName(token);
                  var stringResponse = await response.Content.ReadAsStringAsync();
                  cacheKeys.Add("Descriptor_" + appName);
              // var aa = new ApplicationDescriptor();
              // aa.AppName = "Person-Cup App";
              // aa.Datasets = new List<DatasetDescriptor>();
              // aa.Datasets.Add(new DatasetDescriptor());
              // aa.Datasets.Add(new DatasetDescriptor());
              // var bb = JsonConvert.SerializeObject(aa);
              // JsonConvert.DeserializeObject<ApplicationDescriptor>(bb);
              return JsonConvert.DeserializeObject<ApplicationDescriptor>(stringResponse);
              });
        }
        public static async Task<Dictionary<long, RightsEnum>> GetRightsFromCacheAsync(IMemoryCache cache, IAccountService _accountService, string appName, long userId, string token)
        {
            
            return await
                cache.GetOrCreateAsync("Rights_" + appName + '_' + userId, async entry =>
              {
                  entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                  var response = await _accountService.GetRightsByUserId(token);
                  var stringResponse = await response.Content.ReadAsStringAsync();
                  cacheKeys.Add("Rights_" + appName + '_' + userId);
                  return JsonConvert.DeserializeObject<Dictionary<long, RightsEnum>>(stringResponse);
              });
        }
        public static void RemoveRightsFromCache(IMemoryCache cache, string appName, long rightsId)
        {
            foreach (var key in cacheKeys)
            {
                if(key.StartsWith("Rights_"))
                    cache.Remove(key);
            }
            //cache.Remove("Rights_" + appName + '_' + rightsId);
        }
    }
}