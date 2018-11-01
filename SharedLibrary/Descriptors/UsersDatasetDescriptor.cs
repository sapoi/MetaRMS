using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Descriptors
{
    public class UsersDatasetDescriptor
    {
        public string Name { get; set; }
        public AttributeDescriptor UsernameAttributes { get; set; }
        public AttributeDescriptor PasswordAttributes { get; set; }
        public List<AttributeDescriptor> Attributes { get; set; }
    }
}