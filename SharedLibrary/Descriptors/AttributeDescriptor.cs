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
        // attr type
        public string Type { get; set; }
        // true if can be null
        public bool? Nullable { get; set; }
        // true if should increment automatically - TODO jak jako
        public bool? AutoIncrement { get; set; }
        // true if must be unique
        public bool? Unique { get; set; }
        // password settings for required length and count of certain character types
        public int? MinChar { get; set; }
        public int? MaxChar { get; set; }
        public int? MinNumber { get; set; }
        public int? MaxNumber { get; set; }
        public int? MinLowerCase { get; set; }
        public int? MaxLowerCase { get; set; }
        public int? MinUpperCase { get; set; }
        public int? MaxUpperCase { get; set; }
        // for attributes of type reference to another dataset - means count of ceferences possible
        public int? Min { get; set; }
        public int? Max { get; set; }
    }
}