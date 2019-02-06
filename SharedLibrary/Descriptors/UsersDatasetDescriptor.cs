using System.Collections.Generic;

namespace SharedLibrary.Descriptors
{
    /// <summary>
    /// UserDatasetDescriptor class describes system users dataset.
    /// This descriptor is used whenever accessing system users dataset within the project.
    /// </summary>
    public class UsersDatasetDescriptor
    {
        /// <summary>
        /// Name is an arbitrary string representing name of system users dataset.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description is an arbitrary string describing the dataset.
        /// It is displayed next to a dataset name when the list of users is displayed.
        /// This string is for applications users to help them understand the users dataset.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// PasswordAttribute contains information about password.
        /// </summary>
        public AttributeDescriptor PasswordAttribute { get; set; }
        /// <summary>
        /// List of user defined attributes. Exactly one attribute with Type == "username" is required.
        /// </summary>
        public List<AttributeDescriptor> Attributes { get; set; }
    }
}