using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Descriptors
{
    // class describing datasets for one application
    public class ApplicationDescriptor
    {
        // does not have to be unique
        public string AppName { get; set; }
        // have to be unique
        public string LoginAppName { get; set; }
        public string DefaultLanguage { get; set; }
        public SystemDatasetDescriptor SystemDatasets { get; set; }
        public List<DatasetDescriptor> Datasets { get; set; }
        // public DatasetDescriptor GetDataset(String name){
        //     foreach (DatasetDescriptor dataset in Datasets){
        //         if (dataset.Name == name) return dataset;
        //     }
        //     return null;
        // }
        // public ApplicationDescriptor CreateMockup()
        // {
        //     ApplicationDescriptor a = new ApplicationDescriptor();
        //     a.AppName = "Person-Cup App";
        //     DatasetDescriptor d = new DatasetDescriptor();
        //     d.Name = "User";
        //     AttributeDescriptor a1 = new AttributeDescriptor();
        //     a1.Name="rights";
        //     a1.Type="string";
        //     d.Attributes = new List<AttributeDescriptor> { {a1}};
        //     a.Datasets = new List<DatasetDescriptor>{{d}};
        //     return a;
        // }
    }
}
