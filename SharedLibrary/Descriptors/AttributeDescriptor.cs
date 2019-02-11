namespace SharedLibrary.Descriptors
{
    /// <summary>
    /// AttributeDescriptor class describes single one attribute of a dataset.
    /// This descriptor is used whenever accessing dataset attribute within the project.
    /// </summary>
    public class AttributeDescriptor 
    {
        /// <summary>
        /// Name property.
        /// </summary>
        /// <value>Name is a unique attribute identifier within its dataset.</value>
        public string Name { get; set; }
        /// <summary>
        /// Description property.
        /// </summary>
        /// <value>
        /// Attribute description is an arbitrary string describing the attribute.
        /// It is displayed next to an attribute name when creating or editing dataset.
        /// This string is for applications users to help them fill the attribute value right,
        /// and may contain description of what the attribute means, information about allowed length of input data, etc.
        /// </value>
        public string Description { get; set; }
        /// <summary>
        /// Type property.
        /// </summary>
        /// <value>
        /// Attribute type defines type of data that the attribute contains.
        /// This value can be a simple data type from Enums.AttributeType or a reference.
        /// References can be to arbitrary user defined dataset or system users dataset by using its name as a type value.
        /// </value>
        public string Type { get; set; }
        /// <summary>
        /// Required property.
        /// </summary>
        /// <value>
        /// If Required value is set to true, the attribute value is required to be filled.
        /// </value>
        public bool? Required { get; set; }
        /// <summary>
        /// Unique property.
        /// </summary>
        /// <value>
        /// If Unique value is set to true, the attribute value is required to be unique within the dataset.
        /// </value>
        //TODO not yet supported in code
        public bool? Unique { get; set; }
        /// <summary>
        /// Safer property.
        /// </summary>
        /// <value>
        /// Safer value is used only for Type == "password" attributes, otherwise this value is ignored.
        /// If Safer value is set to true, it is required that all the applications passwords contain at
        /// least 1 upper case, 1 lower case letter and 1 number.
        /// </value>
        public bool? Safer { get; set; }
        /// <summary>
        /// Min property.
        /// </summary>
        /// <value>
        /// For simple data type attributes, Min value means for numeric types (int, float, year) minimal value
        /// and for text types (text, string, username, password) means minimal string length.
        /// </value>
        public int? Min { get; set; }
        /// <summary>
        /// Max property.
        /// </summary>
        /// <value>
        /// For simple data type attributes, Max value means for numeric types (int, float, year) maximal value
        /// and for text types (text, string, username, password) means maximal string length.
        /// For reference types it means maximum of references that the attribute can contain.
        /// </value>
        public int? Max { get; set; }
    }
}