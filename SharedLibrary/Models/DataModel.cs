using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;

namespace SharedLibrary.Models
{
    [Table("data")]
    public class DataModel
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
        [Column("dataset_id")]
        public long DatasetId { get; set; }
        [Required]
        [Column("data")]
        public string Data { get; set; }



     	public Dictionary<String,Object> DataDictionary
         {
             get
             {
                return JsonConvert.DeserializeObject<Dictionary<String,Object>>(Data);
             }
         }
    }
}