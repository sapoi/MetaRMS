using System.Collections.Generic;
using SharedLibrary.Enums;
using SharedLibrary.Descriptors;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorWebApp.Structures
{
    /// <summary>
    /// GetBuilderPartialData class contains properties necessary for _GetBuilderPartial.cshtml file
    /// to display dataset data for get requests in a correct form.
    /// </summary>
    public class GetBuilderPartialData
    {
        /// <summary>
        /// Content that should be displayed in the cell.
        /// </summary>
        /// <value>List of objects</value>
        public List<object> CellContent { get; set; }
        /// <summary>
        /// Attribute the content belongs to. This value is necessary for distinguishing between basic and reference types
        /// and for reference types between User dataset and user-defined datasets.
        /// </summary>
        /// <value>AttributeDescriptor</value>
        public AttributeDescriptor Attribute { get; set; }
        /// <summary>
        /// Application descriptor to get the user dataset name from.
        /// </summary>
        /// <value>ApplicationDescriptor</value>
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
    }
}