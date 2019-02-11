using System.Linq;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace Server.Repositories
{
    /// <summary>
    /// Repository classes are here to be the only and only one access poins to the database
    /// This one - BaseRepository is a parent of all the repositories, containing DatabaseContext
    /// and GetById(long id) method.
    /// Every repository class needs to inherit from this one BaseRepository class and call this 
    /// class' constructor and pass DatabaseContext as well as DbSet parametres on instantiation.
    /// </summary>
    /// <typeparam name="Model">DbSet type.</typeparam>
    public abstract class BaseRepository<Model> where Model : class, IBaseModel
    {
        /// <summary>
        /// Database context used to access database.
        /// </summary>
        protected DatabaseContext databaseContext;
        /// <summary>
        /// Model of the database table.
        /// </summary>
        protected DbSet<Model> model;
        /// <summary>
        /// BaseRepository constructor.
        /// </summary>
        /// <param name="databaseContext">Database context.</param>
        /// <param name="model">Database table model.</param>
        protected BaseRepository(DatabaseContext databaseContext, DbSet<Model> model)
        {
            this.databaseContext = databaseContext;
            this.model = model;
        }
        /// <summary>
        /// This method is used when the id does not need to be checked if valid.
        /// This method should not be used with id from user input, before previous validation.
        /// </summary>
        /// <param name="id">Model id.</param>
        /// <returns>Model with id from parameter if such exists.</returns>
        public Model GetById(long id)
        {
            return model.FirstOrDefault(m => m.Id == id);
        }
        /// <summary>
        /// Method for adding one model to the database.
        /// </summary>
        /// <param name="model">Model to be inserted to the database.</param>
        /// <returns>Number of rows affected.</returns>
        public int Add(Model model)
        {
            this.model.Add(model);
            return databaseContext.SaveChanges();
        }
        /// <summary>
        /// Method for removing one model from the database.
        /// </summary>
        /// <param name="model">Model to be deleted.</param>
        /// <returns>Number of rows affected.</returns>
        public int Remove(Model model)
        {
            this.model.Remove(model);
            return databaseContext.SaveChanges();
        }
    }
}