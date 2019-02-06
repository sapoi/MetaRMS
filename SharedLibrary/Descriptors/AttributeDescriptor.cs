namespace SharedLibrary.Descriptors
{
    /// <summary>
    /// AttributeDescriptor class describes single one attribute of a dataset.
    /// This descriptor is used whenever accessing dataset attribute within the project.
    /// </summary>
    public class AttributeDescriptor 
    {
        /// <summary>
        /// Name is a unique attribute identifier within its dataset.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Attribute description is an arbitrary string describing the attribute.
        /// It is displayed next to an attribute name when creating or editing dataset.
        /// This string is for applications users to help them fill the attribute value right,
        /// and may contain description of what the attribute means, information about allowed length of input data, etc.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Attribute type defines type of data that the attribute contains.
        /// This value can be a simple data type from Enums.AttributeType or a reference.
        /// References can be to arbitrary user defined dataset or system users dataset by using its name as a type value.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// If Required value is set to true, the attribute value is required to be filled.
        /// </summary>
        public bool? Required { get; set; }
        /// <summary>
        /// If Unique value is set to true, the attribute value is required to be unique within the dataset.
        /// </summary>
        //TODO not yet supported in code
        public bool? Unique { get; set; }
        // password settings - if set to true, at least one number, one lower and one uppercase letter is required
        /// <summary>
        /// Safer value is used only for Type == "password" attributes, otherwise this value is ignored.
        /// If Safer value is set to true, it is required that all the applications passwords contain at
        /// least 1 upper case, 1 lower case letter and 1 number.
        /// </summary>
        public bool? Safer { get; set; }
        /// <summary>
        /// For simple data type attributes, Min value means for numeric types (int, float, year) minimal value
        /// and for text types (test, string, username, password) means minimal string length.
        /// </summary>
        //TODO for references is Min not yet supported
        public int? Min { get; set; }
        /// <summary>
        /// For simple data type attributes, Max value means for numeric types (int, float, year) maximal value
        /// and for text types (test, string, username, password) means maximal string length.
        /// For reference types it means maximum of references that the attribute can contain.
        /// </summary>
        public int? Max { get; set; }
    }
}