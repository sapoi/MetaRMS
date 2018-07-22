using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;

namespace DatabaseAccessLibrary.Models
{
    [Table("Table")]
    public class DataColumnModel
    {
        [Key]
        public int Id { get;  set; }
        [Required]
        public string Name { get;  set; }

        public string AllOtherData { get;  set; }
    }
}