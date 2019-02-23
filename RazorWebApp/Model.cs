using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace Server
{
    public class DatabaseContext : DbContext
    {
        public DbSet<ApplicationModel> ApplicationDbSet { get; set; }
        public DbSet<UserModel> UserDbSet { get; set; }
        public DbSet<DataModel> DataDbSet { get; set; }
        public DbSet<RightsModel> RightsDbSet { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // OnDelete ApplicationModel
            modelBuilder.Entity<ApplicationModel>()
                        .HasMany(a => a.Users)
                        .WithOne(u => u.Application)
                        .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ApplicationModel>()
                        .HasMany(a => a.Rights)
                        .WithOne(r => r.Application)
                        .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ApplicationModel>()
                        .HasMany(a => a.Datas)
                        .WithOne(d => d.Application)
                        .OnDelete(DeleteBehavior.Cascade);
            // OnDelete DataModel
            modelBuilder.Entity<DataModel>()
                        .HasOne(d => d.Application)
                        .WithMany(a => a.Datas)
                        .OnDelete(DeleteBehavior.Restrict);
            // OnDelete RightsModel
            modelBuilder.Entity<RightsModel>()
                        .HasOne(r => r.Application)
                        .WithMany(a => a.Rights)
                        .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RightsModel>()
                        .HasMany(r => r.Users)
                        .WithOne(u => u.Rights)
                        .OnDelete(DeleteBehavior.Restrict);
            // OnDelete UserModel
            modelBuilder.Entity<UserModel>()
                        .HasOne(u => u.Application)
                        .WithMany(a => a.Users)
                        .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserModel>()
                        .HasOne(u => u.Rights)
                        .WithMany(r => r.Users)
                        .OnDelete(DeleteBehavior.Restrict);
        }

    }
}