using System.Collections.Generic;

namespace SharedLibrary.Models
{
    /// <summary>
    /// IBaseModelWithApplicationAndData is a interface for all models with ApplicationId and Data.
    /// </summary>
    public interface IBaseModelWithApplicationAndData : IBaseModel
    {
        /// <summary>
        /// ApplicationIn property.
        /// </summary>
        /// <value>ApplicationId represents an Id of application from applications database table.</value>
        long ApplicationId { get; set; }
        /// <summary>
        /// Application property.
        /// </summary>
        /// <value>Application represents an application from applications database table.</value>
        ApplicationModel Application { get; set; }
        /// <summary>
        /// Data property.
        /// </summary>
        /// <value>Represents serialized dictionary of data as defined in application descriptor.</value>
        string Data { get; set; }
        /// <summary>
        /// DataDictionary property.
        /// </summary>
        /// <value>Represents deserialized Data.</value>
        Dictionary<string, List<object>> DataDictionary { get; }
    }
}
