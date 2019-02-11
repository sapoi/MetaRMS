using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using SharedLibrary.Enums;

namespace SharedLibrary.Models
{
    /// <summary>
    /// UserModel is a model of one application user from database users table.
    /// </summary>
    [Table("users")]
    public class UserModel : IBaseModelWithApplicationAndData
    {
        /// <summary>
        /// Id property.
        /// </summary>
        /// <value>Id is a unique identificator of user in database.</value>
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
        /// Password property.
        /// </summary>
        /// <value>Contains user's hashed password.</value>
        [Required]
        [Column("password")]
        public string Password { get; set; }
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
        [JsonIgnore]
        public Dictionary<string, List<object>> DataDictionary
        {
            get
            {
            return JsonConvert.DeserializeObject<Dictionary<string, List<object>>>(Data);
            }
        }
        /// <summary>
        /// RightsId property.
        /// </summary>
        /// <value>RightsId represents an Id of rights from rights database table.</value>
        [Required]
        [Column("rights_id")]
        public long RightsId { get; set; }
        /// <summary>
        /// Rights property.
        /// </summary>
        /// <value>Rights represents a rights from rights database table.</value>
        [ForeignKey("RightsId")]
        public RightsModel Rights { get; set; }
        /// <summary>
        /// Language property.
        /// </summary>
        /// <value>Language represents user's language of the application.</value>
        [Required]
        [Column("language")]
        public LanguageEnum Language { get; set; }
        /// <summary>
        /// GetUsername method.
        /// </summary>
        /// <returns>Returns username of the user from Data.</returns>
        public string GetUsername()
        {
            string userAttributeName = this.Application.GetUsernameAttribute().Name;
            var usernameObject = this.DataDictionary[userAttributeName].FirstOrDefault();
            if (usernameObject == null)
                return "";
            return usernameObject.ToString();
        }
    }
}
