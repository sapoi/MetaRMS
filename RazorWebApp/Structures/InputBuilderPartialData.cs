using System.Collections.Generic;
using SharedLibrary.Enums;
using SharedLibrary.Descriptors;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorWebApp.Structures
{
    public class InputBuilderPartialData
    {
        public AttributeDescriptor Attribute;
        public string OutValue;
        public List<object> InValue;
        public Dictionary<string, List<SelectListItem>> SelectData;
    }
}