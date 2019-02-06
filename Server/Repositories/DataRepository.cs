using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SharedLibrary.Models;

namespace Server.Repositories
{
    public class DataRepository: BaseRepository<DataModel>
    {
        public DataRepository(DatabaseContext databaseContext):base(databaseContext, databaseContext.DataDbSet) { }
        /// <summary>
        /// This method is used when the dataId does not need to be checked if 
        /// it is from a correct dataset and application.
        /// </summary>
        /// <param name="dataId"></param>
        /// <returns></returns>
        public DataModel GetById(long dataId)
        {
            return _model.FirstOrDefault(d => d.Id == dataId);
        }
        /// <summary>
        /// This method is used in controllers where to get requested data by id only if 
        /// also the application id and the dataset if is correct.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="datasetId"></param>
        /// <param name="dataId"></param>
        /// <returns></returns>
        public DataModel GetById(long applicationId, long datasetId, long dataId)
        {
            return _model.FirstOrDefault(d => d.ApplicationId == applicationId &&
                                              d.DatasetId == datasetId &&
                                              d.Id == dataId);
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