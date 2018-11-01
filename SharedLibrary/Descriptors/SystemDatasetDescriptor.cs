using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SharedLibrary.Descriptors;

namespace SharedLibrary.Descriptors
{
    // class describing datasets for one application
    public class SystemDatasetDescriptor
    {
        public UsersDatasetDescriptor UsersDatasetDescriptor { get; set; }
    }
}
