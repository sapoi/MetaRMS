using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedLibrary;
using SharedLibrary.Descriptors;
using SharedLibrary.Models;

namespace DatabaseAccessLibrary
{
    public static class DescriptorCache
    {
        private static List<ApplicationDescriptor> cachedApplicationDescriptors;
        private static DbContext _context;
        static DescriptorCache()
        {
            cachedApplicationDescriptors = new List<ApplicationDescriptor>();
        }
        
        public static DatasetDescriptor GetDatasetDescriptor(DbContext context, string applicationName, string datasetName)
        {
            _context = context;
            ApplicationDescriptor applicationDescriptor = cachedApplicationDescriptors.Find(a => a.AppName == applicationName);
            if (applicationDescriptor != null)
            {
                DatasetDescriptor datasetDescriptor = applicationDescriptor.Datasets.Find(d => d.Name == datasetName);
                if (datasetDescriptor != null)
                    return datasetDescriptor;
            }
            ApplicationModel newApplication = (ApplicationModel)_context.Find(typeof(ApplicationModel), applicationName);
            if (newApplication == null)
                throw new DatasetNotFoundException("Application " + applicationName + " not found.");
            ApplicationDescriptor newApplicationDescriptor = JsonConvert.DeserializeObject<ApplicationDescriptor>(newApplication.ApplicationDescriptorJSON);
            cachedApplicationDescriptors.Add(newApplicationDescriptor);
            DatasetDescriptor newDatasetDescriptor = newApplicationDescriptor.Datasets.Find(d => d.Name == datasetName);
            if (newDatasetDescriptor == null)
                throw new DatasetNotFoundException("Dataset " + datasetName + " in application " + applicationName + " not found.");
            return newDatasetDescriptor;
        }
    }
}