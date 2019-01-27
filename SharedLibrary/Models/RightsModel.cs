using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SharedLibrary.Models
{
    [Table("rights")]
    public class RightsModel: BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Required]
        [Column("application_id")]
        public long ApplicationId { get; set; }
        [ForeignKey("ApplicationId")]
        public ApplicationModel Application { get; set; }
        [Required]
        [Column("name")]
        public string Name { get; set; }
        [Required]
        [Column("data")]
        public string Data { get; set; }

        [InverseProperty("Rights")]
        public List<UserModel> Users { get; set; }

        [JsonIgnore]
        public Dictionary<string,int> DataDictionary
         {
             get
             {
                return JsonConvert.DeserializeObject<Dictionary<string,int>>(Data);
             }
         }
    }
}