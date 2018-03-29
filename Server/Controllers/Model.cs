using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace Server.Controllers
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Application> ApplicationsDbSet { get; set; }
        public DbSet<User> UsersDbSet { get; set; }
        public DbSet<Data> DataDbSet { get; set; }
        public DbSet<Rights> RightsDbSet { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        
    }
}