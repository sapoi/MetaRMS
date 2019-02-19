using System.Collections.Generic;
using SharedLibrary.Enums;
using SharedLibrary.Descriptors;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorWebApp.Structures
{
    public class EditBuilderPartialData
    {
        public AttributeDescriptor Attribute { get; set; }
        public string OutValue { get; set; }
        public List<string> InValue { get; set; }
        public Dictionary<string, List<SelectListItem>> SelectData { get; set; }
    }
}