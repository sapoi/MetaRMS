using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
    public class User
    {
        [Key]
        public long Id { get; set; }
        public string ApplicationName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public string JsonUserData { get; set; }
    }
}
