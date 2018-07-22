using DatabaseAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseAccessLibrary.DbContexts
{
    public class DataColumnDbContext : DbContext
    {
        public DbSet<DataColumnModel> DataColumnModel {get;set;}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=DataColumnDB.db");
        }
    }
}