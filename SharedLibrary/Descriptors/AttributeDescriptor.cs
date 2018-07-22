using System;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Descriptors
{
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
}