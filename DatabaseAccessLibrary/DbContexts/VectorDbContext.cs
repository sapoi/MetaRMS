using DatabaseAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseAccessLibrary.DbContexts
{
public class VectorDbContext : DbContext
    {
        public DbSet<VectorModel> VectorModel {get;set;}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=VectorModelDB.db");
        }
    }
}