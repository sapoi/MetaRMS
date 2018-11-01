using System.Collections.Generic;
using SharedLibrary.Enums;
using SharedLibrary.Descriptors;
using System;

namespace RazorWebApp.Structures
{
    public class InputBuilderPartialData
    {
        public AttributeDescriptor Attribute;
        public string OutValue;
        public string InValue;
        public Dictionary<string, List<Object>> SelectData;
    }
}