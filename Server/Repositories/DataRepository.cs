using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SharedLibrary.Models;

namespace Server.Repositories
{
    public class DataRepository: BaseRepository<DataModel>
    {
        public DataRepository(DatabaseContext databaseContext):base(databaseContext, databaseContext.DataDbSet) { }
        public DataModel GetById(long id)
        {
            return _model.FirstOrDefault(d => d.Id == id);
        }
        public List<DataModel> GetAllByApplicationIdAndDatasetId(long applicationId, long datasetId)
        {
            return _model.Where(d => d.ApplicationId == applicationId && d.DatasetId == datasetId).ToList();
        }
        public int SetData(DataModel model, Dictionary<string, List<object>> data)
        {
            string JsonData = JsonConvert.SerializeObject(data);
            model.Data = JsonData;
            return _databaseContext.SaveChanges();
        }
    }
}