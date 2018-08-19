using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SharedLibrary.Models
{
    [Table("users")]
    public class UserModel
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
        [Column("username")]
        public string Username { get; set; }
        [Required]
        [Column("password")]
        public string Password { get; set; }
        [Required]
        [Column("data")]
        public string Data { get; set; }
        [JsonIgnore]
        public Dictionary<String, Object> DataDictionary
         {
             get
             {
                return JsonConvert.DeserializeObject<Dictionary<String,Object>>(Data);
             }
         }
        [Required]
        [Column("rights_id")]
        public long RightsId { get; set; }
        [ForeignKey("RightsId")]
        public RightsModel Rights { get; set; }
    }
}
