using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SharedLibrary.Enums;

namespace SharedLibrary.Models
{
    /// <summary>
    /// RightsModel is a model of one application rights from database rights table.
    /// </summary>
    [Table("rights")]
    public class RightsModel : IBaseModel
    {
        /// <summary>
        /// Id property.
        /// </summary>
        /// <value>Id is a unique identificator of rights in database.</value>
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
        /// Name property.
        /// </summary>
        /// <value>Rights name.</value>
        [Required]
        [Column("name")]
        public string Name { get; set; }
        /// <summary>
        /// Data property.
        /// </summary>
        /// <value>Represents serialized dictionary of data as defined in application descriptor.</value>
        [Required]
        [Column("data")]
        public string Data { get; set; }
        /// <summary>
        /// Users property.
        /// </summary>
        /// <value>Users property contains all UserModels associated with the rights.</value>
        [InverseProperty("Rights")]
        public List<UserModel> Users { get; set; }
        /// <summary>
        /// DataDictionary property.
        /// </summary>
        /// <value>Represents deserialized Data.</value>
        [JsonIgnore]
        public Dictionary<long, RightsEnum> DataDictionary
        {
            get
            {
            return JsonConvert.DeserializeObject<Dictionary<long, RightsEnum>>(Data);
            }
        }
    }
}