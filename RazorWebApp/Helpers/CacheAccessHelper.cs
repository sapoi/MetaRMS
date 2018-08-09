using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Services;

namespace RazorWebApp.Helpers
{
    public class CacheAccessHelper
    {
        public static async Task<ApplicationDescriptor> GetApplicationDescriptorFromCacheAsync(IMemoryCache cache, IAccountService _accountService, string appName, string token)
        {
            return await cache.GetOrCreateAsync("Descriptor_" + appName, async entry =>
              {
                  entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                  var response = await _accountService.GetApplicationDescriptorByAppName(token);
                  var stringResponse = await response.Content.ReadAsStringAsync();
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
                  var response = await _accountService.GetUserRightsById(token);
                  var stringResponse = await response.Content.ReadAsStringAsync();
                  return JsonConvert.DeserializeObject<Dictionary<long, RightsEnum>>(stringResponse);
              });
        }
    }
}