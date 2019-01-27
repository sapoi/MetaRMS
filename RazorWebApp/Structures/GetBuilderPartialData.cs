using System.Collections.Generic;
using SharedLibrary.Enums;
using SharedLibrary.Descriptors;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorWebApp.Structures
{
    public class GetBuilderPartialData
    {
        public List<object> CellContent;
        public AttributeDescriptor Attribute;
        public ApplicationDescriptor ApplicationDescriptor;
    }
}