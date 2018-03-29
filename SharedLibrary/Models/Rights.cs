using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    public class Rights
    {
        [Key]
        public long Id { get; set; }
        public string ApplicationName { get; set; }
        public string RightsName { get; set; }
        public string JsonRightsData { get; set; }
    }
}