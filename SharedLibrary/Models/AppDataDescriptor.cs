using System;
using System.Collections.Generic;

namespace SharedLibrary.Models
{
    // class describing datasets for one application
    public class AppDataDescriptor
    {
        public String AppName { get; set; }
        public List<DatasetDescriptor> Datasets { get; set; }
        public DatasetDescriptor GetDataset(String name){
            foreach (DatasetDescriptor dataset in Datasets){
                if (dataset.Name == name) return dataset;
            }
            return null;
        }
        public AppDataDescriptor CreateMockup()
        {
            AppDataDescriptor a = new AppDataDescriptor();
            a.AppName = "Person-Cup App";
            DatasetDescriptor d = new DatasetDescriptor();
            d.Name = "User";
            AttributeDescriptor a1 = new AttributeDescriptor();
            a1.Name="rights";
            a1.Type="string";
            d.Attributes = new List<AttributeDescriptor> { {a1}};
            a.Datasets = new List<DatasetDescriptor>{{d}};
            return a;
        }
    }
    public class DatasetDescriptor
    {
        public String Name { get; set; }
        public List<AttributeDescriptor> Attributes { get; set; }
    }
    public class AttributeDescriptor 
    {
        public String Name { get; set; }
        public string Type { get; set; }
        public String Description { get; set; }
        public RelationshipDescriptor Relationship { get; set; }
        public Boolean Nullable { get; set; }
        public Boolean AutoIncrement { get; set; }
        public Boolean Unique { get; set; }
    }
    public enum RelationshipDescriptor
    {
        none, toone, tomany
    }
}
