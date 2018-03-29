using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    // class describing one application
    public class Application
    {
        [Key]
        public string Name { get; set; }
        public string AppDataDescriptor { get; set; }
    }
}