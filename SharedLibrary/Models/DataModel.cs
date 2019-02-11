using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;

namespace SharedLibrary.Models
{
    /// <summary>
    /// DataModel is a model of one application data from database data table.
    /// </summary>
    [Table("data")]
    public class DataModel : IBaseModelWithApplicationAndData
    {
        /// <summary>
        /// Id property.
        /// </summary>
        /// <value>Id is a unique identificator of data in database.</value>
        [Key]
        [Column("id")]
        public long Id { get; set; }
        /// <summary>
        /// ApplicationIn property.
        /// </summary>
        /// <value>ApplicationId represents an Id of application from applications database table.</value>
        [Required]
        [Column("application_id")]
        public long ApplicationId { get; set; }
        /// <summary>
        /// Application property.
        /// </summary>
        /// <value>Application represents an application from applications database table.</value>
        [ForeignKey("ApplicationId")]
        public ApplicationModel Application { get; set; }
        /// <summary>
        /// DatasetId property.
        /// </summary>
        /// <value>Id of dataset to which the data belongs.</value>
        [Required]
        [Column("dataset_id")]
        public long DatasetId { get; set; }
        /// <summary>
        /// Data property.
        /// </summary>
        /// <value>Represents serialized dictionary of data as defined in application descriptor.</value>
        [Required]
        [Column("data")]
        public string Data { get; set; }
        /// <summary>
        /// DataDictionary property.
        /// </summary>
        /// <value>Represents deserialized Data.</value>
     	public Dictionary<string, List<object>> DataDictionary
        {
            get
            {
            return JsonConvert.DeserializeObject<Dictionary<string, List<object>>>(Data);
            }
        }
    }
}