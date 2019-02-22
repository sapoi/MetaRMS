using System.Collections.Generic;
using SharedLibrary.Enums;
using SharedLibrary.Descriptors;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorWebApp.Structures
{
    /// <summary>
    /// InputBuilderPartialData class contains properties necessary for _InputBuilderPartial.cshtml file
    /// to display dataset data for create and patch requests in a correct form.
    /// </summary>
    public class InputBuilderPartialData
    {
        /// <summary>
        /// Attribute the content belongs to. This value is necessary for distinguishing between data types,
        /// its min and max values, if it is required, ...
        /// </summary>
        /// <value>AttributeDescriptor</value>
        public AttributeDescriptor Attribute { get; set; }
        /// <summary>
        /// In value contains list of string values that are already selected or filled in.
        /// </summary>
        /// <value>List of strings</value>
        public List<string> InValue { get; set; }
        /// <summary>
        /// OutValue is a single string into which the filled or selcted value is stored.
        /// </summary>
        /// <value>string</value>
        public string OutValue { get; set; }
        /// <summary>
        /// SelectData property contains for each referenced dataset its avaliable values that can be selected.
        /// </summary>
        /// <value>Dictionary with referenced dataset as string key and list of SelectListItem as value</value>
        public Dictionary<string, List<SelectListItem>> SelectData { get; set; }
    }
}