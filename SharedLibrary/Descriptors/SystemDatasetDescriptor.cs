namespace SharedLibrary.Descriptors
{
    /// <summary>
    /// SystemDatasetDescriptor class contains system datasets.
    /// This descriptor is used whenever accessing system dataset within the project.
    /// </summary>
    public class SystemDatasetDescriptor
    {
        /// <summary>
        /// UserDatasetDescriptor contains information about application users.
        /// </summary>
        public UsersDatasetDescriptor UsersDatasetDescriptor { get; set; }
    }
}
