using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedLibrary.Models
{
    public class Data
    {
        [Key]
        public long Id { get; set; }
        public string ApplicationName { get; set; }
        public string DatasetName { get; set; }
        public string JsonData { get; set; }
     	public Dictionary<String,Object> data;
        public void LoadFromJson()
        {
            var fromJsonResult = JsonConvert.DeserializeObject<Dictionary<String,Object>>(JsonData);
            this.data = fromJsonResult;
        }
    }
}