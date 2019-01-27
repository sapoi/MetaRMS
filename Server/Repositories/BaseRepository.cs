using System.Linq;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace Server.Repositories
{
    /// <summary>
    /// Repository classes are here to be the only and only one access poins to the database.async
    /// This one - BaseRepository is a parent of all the repositories, containing DatabaseContext
    /// and GetById(long id) method.
    /// Every repository class needs to inherit from this one BaseRepository class and call this 
    /// class' constructor and pass DatabaseContext as well as DbSet parametres on instantiation.
    /// </summary>
    /// <typeparam name="Model">DbSet type.</typeparam>
    public abstract class BaseRepository<Model> where Model : class, BaseModel
    {
        public DatabaseContext _databaseContext;
        public DbSet<Model> _model;
        public BaseRepository(DatabaseContext databaseContext, DbSet<Model> model)
        {
            _databaseContext = databaseContext;
            _model = model;
        }
        // public Model GetById(long id)
        // {
        //     return _model.FirstOrDefault(d => d.Id == id);
        // }
        public int Add(Model model)
        {
            _model.Add(model);
            return _databaseContext.SaveChanges();
        }
         public int Remove(Model model)
        {
            _model.Remove(model);
            return _databaseContext.SaveChanges();
        }
    }
}