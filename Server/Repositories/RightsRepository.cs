using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SharedLibrary.Models;

namespace Server.Repositories
{
    public class RightsRepository: BaseRepository<RightsModel>
    {
        public RightsRepository(DatabaseContext databaseContext):base(databaseContext, databaseContext.RightsDbSet) { }
        public RightsModel GetById(long id)
        {
            return _model.FirstOrDefault(d => d.Id == id);
        }
        public RightsModel GetById(long applicationId, long id)
        {
            return _model.FirstOrDefault(d => d.ApplicationId == applicationId &&
                                              d.Id == id);
        }
        public List<RightsModel> GetByApplicationIdAndName(long applicationId, string name)
        {
            return _model.Where(r => r.ApplicationId == applicationId && 
                                     r.Name == name)
                         .ToList();
        }
        public List<RightsModel> GetAllByApplicationId(long applicationId)
        {
            return _model.Where(r => r.ApplicationId == applicationId).ToList();
        }
        public int SetNameAndData(RightsModel model, string name, Dictionary<string, int> data)
        {
            model.Name = name;
            model.Data = JsonConvert.SerializeObject(data);
            return _databaseContext.SaveChanges();
        }
    }
}