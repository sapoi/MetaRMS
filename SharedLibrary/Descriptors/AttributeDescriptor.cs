using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Descriptors
{
    public class AttributeDescriptor 
    {
        // ttribute name
        public string Name { get; set; }
        // attribute description
        public string Description { get; set; }
        // attribute type
        public string Type { get; set; }
        // true if can not be null
        public bool? Required { get; set; }
        // true if must be unique
        public bool? Unique { get; set; }
        // password settings - if set to true, at least one number, one lower and one uppercase letter is required
        public bool? Safer { get; set; }
        // for attributes of type reference to another dataset - means count of ceferences possible or minimal int/float value
        // and settings for required minimal length for text, string, password and username fields
        public int? Min { get; set; }
        public int? Max { get; set; }
    }
}