using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;

namespace SharedLibrary.Models
{
    /// <summary>
    /// ApplicationModel is a model of one application from database "applications" table.
    /// </summary>
    [Table("applications")]
    public class ApplicationModel : IBaseModel
    {
        /// <summary>
        /// Id property.
        /// </summary>
        /// <value>Id is a unique identificator of application in database.</value>
        [Key]
        [Column("id")]
        public long Id { get; set; }
        /// <summary>
        /// LoginApplicationName property.
        /// </summary>
        /// <value>LoginApplicationName is a unique string representation of a application used for user login.</value>
        [Required]
        [Column("login_application_name")]
        public string LoginApplicationName { get; set; }
        /// <summary>
        /// ApplicationDescriptorJSON property.
        /// </summary>
        /// <value>ApplicationDescriptorJSON contains application descriptor in JSON format.</value>
        [Required]
        [Column("descriptor")]
        public string ApplicationDescriptorJSON { get; set; }
        /// <summary>
        /// Users property.
        /// </summary>
        /// <value>Users property contains all UserModels associated with the application.</value>

        [InverseProperty("Application")]
        public List<UserModel> Users { get; set; }
        /// <summary>
        /// Rights property.
        /// </summary>
        /// <value>Rights property contains all RightsModels associated with the application.</value>
        [InverseProperty("Application")]
        public List<RightsModel> Rights { get; set; }
        /// <summary>
        /// Datas property.
        /// </summary>
        /// <value>Datas property contains all DataModels associated with the application.</value>
        [InverseProperty("Application")]
        public List<DataModel> Datas { get; set; }
        /// <summary>
        /// ApplicationDescriptor property.
        /// </summary>
        /// <value>Deserializer ApplicationDescriptorJSON value.</value>
        [JsonIgnore]
        public ApplicationDescriptor ApplicationDescriptor
        {
            get
            {
                return JsonConvert.DeserializeObject<ApplicationDescriptor>(ApplicationDescriptorJSON);
            }
        }
        /// <summary>
        /// GetUsernameAttribute method.
        /// </summary>
        /// <returns>AttributeDescriptor describing username.</returns>
        public AttributeDescriptor GetUsernameAttribute()
        {
            return this.ApplicationDescriptor.GetUsernameAttribute();
        }
    }
}