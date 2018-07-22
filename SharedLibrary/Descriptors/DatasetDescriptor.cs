using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Descriptors
{
    public class DatasetDescriptor
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public List<AttributeDescriptor> Attributes { get; set; }
    }
}