using System.Linq;
using SharedLibrary.Models;

namespace Server.Repositories
{
    public class ApplicationRepository: BaseRepository<ApplicationModel>
    {
        public ApplicationRepository(DatabaseContext databaseContext):base(databaseContext, databaseContext.ApplicationDbSet) { }
        public ApplicationModel GetById(long id)
        {
            return _model.FirstOrDefault(d => d.Id == id);
        }
        public ApplicationModel GetByLoginApplicationName(string loginApplicationName)
        {
            return _databaseContext.ApplicationDbSet.Where(a => a.LoginApplicationName == loginApplicationName).FirstOrDefault();
        }
        
    }
}