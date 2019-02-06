using System.Collections.Generic;
using System.Linq;

namespace SharedLibrary.Descriptors
{
    /// <summary>
    /// ApplicationDescriptor class describes single one application.
    /// This descriptor is used whenever accessing application within the project.
    /// </summary>
    public class ApplicationDescriptor
    {
        /// <summary>
        /// ApplicationName is a non-unique string displayed for a logged user.
        /// </summary>
        public string ApplicationName { get; set; }
        /// <summary>
        /// LoginApplicationName is a unique string used to log into the application.
        /// </summary>
        public string LoginApplicationName { get; set; }
        //TODO remove?
        public string DefaultLanguage { get; set; }
        /// <summary>
        /// SystemDatasets contains system datasets with mandatory elements and some user defined parts.
        /// In current version UserDatasetDescriptor is obligatory.
        /// </summary>
        public SystemDatasetDescriptor SystemDatasets { get; set; }
        /// <summary>
        /// Datasets list contains user defined datasets.
        /// </summary>
        public List<DatasetDescriptor> Datasets { get; set; }
        /// <summary>
        /// Function used to get an AttributeDescriptor for username.
        /// This descriptor can be found as one of the SystemDatasets.UsersDatasetDescriptor.Attributes
        /// and  it is the only one with a .Type == "username".
        /// </summary>
        /// <returns>The return value is an AttributeDescriptor describing username.</returns>
        public AttributeDescriptor GetUsernameAttribute()
        {
            return this.SystemDatasets.UsersDatasetDescriptor.Attributes.First(a => a.Type == "username");
        }
    }
}
