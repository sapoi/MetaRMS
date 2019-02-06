using System.Collections.Generic;

namespace SharedLibrary.Descriptors
{
    /// <summary>
    /// DatasetDescriptor class describes single one user defined dataset.
    /// This descriptor is used whenever accessing user defined dataset within the project.
    /// </summary>
    public class DatasetDescriptor
    {
        /// <summary>
        /// Dataset name is unique within the application datasets.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Dataset description is an arbitrary string describing the dataset.
        /// It is displayed next to a dataset name when the list of dataset data is displayed.
        /// This string is for applications users to help them understand what the dataset contains.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Id is a unique dataset identifier within application and is assigned to the dataset automatically.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// List of Dataset attributes.
        /// </summary>
        public List<AttributeDescriptor> Attributes { get; set; }
    }
}