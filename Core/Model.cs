using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace Core
{
    public class DatabaseContext : DbContext
    {
        /// <summary>
        /// Database set representing applications table.
        /// </summary>
        /// <value>ApplicationModel DbSet</value>
        public DbSet<ApplicationModel> ApplicationDbSet { get; set; }
        /// <summary>
        /// Database set representing users table.
        /// </summary>
        /// <value>UserModel DbSet</value>
        public DbSet<UserModel> UserDbSet { get; set; }
        /// <summary>
        /// Database set representing data table.
        /// </summary>
        /// <value>DataModel DbSet</value>
        public DbSet<DataModel> DataDbSet { get; set; }
        /// <summary>
        /// Database set representing rights table.
        /// </summary>
        /// <value>RightsModel DbSet</value>
        public DbSet<RightsModel> RightsDbSet { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        /// <summary>
        /// This method sets delete behavior to each model.
        /// </summary>
        /// <param name="modelBuilder">ModelBuilder to use</param>
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