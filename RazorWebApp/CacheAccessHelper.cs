using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using SharedLibrary.Services;

public class CacheAccessHelper
{

    public static async Task<ApplicationDescriptor> GetApplicationDescriptorFromCacheAsync(IMemoryCache cache, IAccountService _accountService, string appName, string token)
        {
            return await 
                cache.GetOrCreateAsync(appName+"Descriptor", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                var response = await _accountService.GetApplicationDescriptor(appName, token);
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
}