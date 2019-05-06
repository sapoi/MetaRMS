using System.Linq;
using SharedLibrary.Models;

namespace Core.Repositories
{
    /// <summary>
    /// Repository for ApplicationModels.
    /// </summary>
    public class ApplicationRepository: BaseRepository<ApplicationModel>
    {
        /// <summary>
        /// ApplicationRepository constructor calling BaseRepository constructor.
        /// </summary>
        public ApplicationRepository(DatabaseContext databaseContext):base(databaseContext, databaseContext.ApplicationDbSet) { }
        /// <summary>
        /// GetByLoginApplicationName method looks for applications with the same LoginApplicationName as from the parameter.
        /// </summary>
        /// <param name="loginApplicationName">Name of application to look for</param>
        /// <returns>Application with the same LoginApplicationName as from the parameter, if such was found.</returns>
        public ApplicationModel GetByLoginApplicationName(string loginApplicationName)
        {
            return databaseContext.ApplicationDbSet.Where(a => a.LoginApplicationName == loginApplicationName).FirstOrDefault();
        }
        
    }
}