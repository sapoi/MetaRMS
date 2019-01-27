using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;

namespace SharedLibrary.Models
{
    [Table("data")]
    public class DataModel : BaseModelWithApplicationAndData
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
     	public Dictionary<string, List<object>> DataDictionary
         {
             get
             {
                return JsonConvert.DeserializeObject<Dictionary<string, List<object>>>(Data);
             }
         }
    }
}