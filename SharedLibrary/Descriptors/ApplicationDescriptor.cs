using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Enums;

namespace SharedLibrary.Descriptors
{
    /// <summary>
    /// ApplicationDescriptor class describes single one application.
    /// This descriptor is used whenever accessing application within the project.
    /// </summary>
    public class ApplicationDescriptor
    {
        /// <summary>
        /// ApplicationName property.
        /// </summary>
        /// <value>ApplicationName is a non-unique string displayed for a logged user.</value>
        public string ApplicationName { get; set; }
        /// <summary>
        /// LoginApplicationName property.
        /// </summary>
        /// <value>LoginApplicationName is a unique string used to log into the application.</value>
        public string LoginApplicationName { get; set; }
        /// <summary>
        /// DefaultLanguage property.
        /// </summary>
        /// <value>DefaultLanguage is a value from LanguageEnum, used when creating a new user as the users language.</value>
        public LanguageEnum DefaultLanguage { get; set; }
        /// <summary>
        /// SystemDatasets property.
        /// </summary>
        /// <value>
        /// SystemDatasets contains system datasets with mandatory elements and some user defined parts.
        /// In current version UserDatasetDescriptor is obligatory.
        /// </value>
        public SystemDatasetDescriptor SystemDatasets { get; set; }
        /// <summary>
        /// Datasets property.
        /// </summary>
        /// <value>Datasets list contains user-defined datasets.</value>
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
