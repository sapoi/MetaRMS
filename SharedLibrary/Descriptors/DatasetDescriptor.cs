using System.Collections.Generic;

namespace SharedLibrary.Descriptors
{
    /// <summary>
    /// DatasetDescriptor class describes single one user-defined dataset.
    /// This descriptor is used whenever accessing user-defined dataset within the project.
    /// </summary>
    public class DatasetDescriptor
    {
        /// <summary>
        /// Name property.
        /// </summary>
        /// <value>
        /// Dataset name is unique within the application datasets.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Description property.
        /// </summary>
        /// <value>
        /// Dataset description is an arbitrary string describing the dataset.
        /// It is displayed next to a dataset name when the list of dataset data is displayed.
        /// This string is for applications users to help them understand what the dataset contains.
        /// </value>
        public string Description { get; set; }
        /// <summary>
        /// Id property.
        /// </summary>
        /// <value>
        /// Id is a unique dataset identifier within application and is assigned to the dataset automatically.
        /// </value>
        public long Id { get; set; }
        /// <summary>
        /// Attributes property.
        /// </summary>
        /// <value>
        /// List of Dataset attributes.
        /// </value>
        public List<AttributeDescriptor> Attributes { get; set; }
    }
}