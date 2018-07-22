using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;

namespace DatabaseAccessLibrary.Models
{
    public class FullDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<FullModel> FullModel {get;set;}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=FullModelDB.db");
        }
    }
    [Table("Table")]
    public class FullModel
    {
        [Key]
        public int Id { get;  set; }
        [Required]
        public string Name { get;  set; }

        [Required]
        public string Surname { get;  set; }
        [Required]
        public DateTime Birthdate { get;  set; }
        [Required]
        public int Age { get;  set; }
        [Required]
        public bool Married { get;  set; }
        [Required]
        public double Height { get;  set; }
        [Required]
        public long Salary { get;  set; }
        [Required]
        public ABC Abc { get;  set; }

        public string NullSurname { get;  set; }
        public DateTime? NullBirthdate { get;  set; }
        public int? NullAge { get;  set; }
        public bool? NullMarried { get;  set; }
        public double? NullHeight { get;  set; }
        public long? NullSalary { get;  set; }
        public ABC? NullAbc { get;  set; }

        public string ToXML()
        {
            var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(this.GetType());
            serializer.Serialize(stringwriter, this);
            return stringwriter.ToString();
        }

        public static FullModel LoadFromXMLString(string xmlText)
        {
            var stringReader = new System.IO.StringReader(xmlText);
            var serializer = new XmlSerializer(typeof(FullModel ));
            return serializer.Deserialize(stringReader) as FullModel ;
        }
    }
    public enum ABC
    {
        A, B, C,
    }
}