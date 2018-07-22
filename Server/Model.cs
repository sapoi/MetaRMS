using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace Server
{
    public class DatabaseContext : DbContext
    {
        public DbSet<ApplicationModel> ApplicationsDbSet { get; set; }
        public DbSet<UserModel> UsersDbSet { get; set; }
        public DbSet<DataModel> DataDbSet { get; set; }
        public DbSet<RightsModel> RightsDbSet { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

    }
}