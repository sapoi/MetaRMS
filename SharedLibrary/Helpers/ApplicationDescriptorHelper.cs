using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;

namespace SharedLibrary.Helpers
{
    // class describing datasets for one application
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationDescriptorHelper
    {
        // // public getter for descriptor
        // public ApplicationDescriptor Descriptor 
        // { 
        //     get 
        //     { 
        //         return descriptor; 
        //     }
        // }
        // private decsriptor
        ApplicationDescriptor descriptor;
        public ApplicationDescriptorHelper(ApplicationDescriptor descriptor)
        {
            this.descriptor = descriptor;
        }
        // public ApplicationDescriptorHelper(string descriptor)
        // {
        //     this.descriptor = JsonConvert.DeserializeObject<ApplicationDescriptor>(descriptor);
        // }
        public DatasetDescriptor GetDatasetDescriptorByName(string datasetName)
        {
            return descriptor.Datasets.FirstOrDefault(d => d.Name == datasetName);
        }
        // // if datasetName is not in application, returns null
        // public long? GetDatasetIdByName(string datasetName)
        // {
        //     var dataset = descriptor.Datasets.Find(d => d.Name == datasetName);
        //     if (dataset == null)
        //         return null;
        //     return dataset.Id;
        // }
    }
}