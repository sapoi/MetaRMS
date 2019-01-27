using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;

namespace SharedLibrary.Models
{
    // class describing one application
    [Table("applications")]
    public class ApplicationModel : BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Required]
        [Column("login_application_name")]
        public string LoginApplicationName { get; set; }
        [Required]
        [Column("descriptor")]
        public string ApplicationDescriptorJSON { get; set; }

        [InverseProperty("Application")]
        public List<UserModel> Users { get; set; }
        [InverseProperty("Application")]
        public List<RightsModel> Rights { get; set; }
        [InverseProperty("Application")]
        public List<DataModel> Datas { get; set; }
        [JsonIgnore]
        public ApplicationDescriptor ApplicationDescriptor
        {
            get
            {
                return JsonConvert.DeserializeObject<ApplicationDescriptor>(ApplicationDescriptorJSON);
            }
        }
        public AttributeDescriptor GetUsernameAttribute()
        {
            return this.ApplicationDescriptor.GetUsernameAttribute();
        }
    }
}