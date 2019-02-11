namespace SharedLibrary.Descriptors
{
    /// <summary>
    /// UserDatasetDescriptor class describes system users dataset.
    /// This descriptor is used whenever accessing system users dataset within the project.
    /// </summary>
    public class UsersDatasetDescriptor : DatasetDescriptor
    {
        /// <summary>
        /// PasswordAttribute property.
        /// </summary>
        /// <value>
        /// PasswordAttribute contains information about password.
        /// </value>
        public AttributeDescriptor PasswordAttribute { get; set; }
    }
}