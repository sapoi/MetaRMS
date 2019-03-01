using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedLibrary.Models;
using System.Linq.Dynamic.Core;

namespace RazorWebApp.Repositories
{
    /// <summary>
    /// Repository for DataModels.
    /// </summary>
    /// <typeparam name="DataModel">The type of database model</typeparam>
    public class DataRepository: BaseRepository<DataModel>
    {
        /// <summary>
        /// DataRepository constructor calling BaseRepository constructor.
        /// </summary>
        /// <param name="databaseContext"></param>
        public DataRepository(DatabaseContext databaseContext):base(databaseContext, databaseContext.DataDbSet) { }
        /// <summary>
        /// This method is used in controllers to get requested data by id only if also the application id and the dataset if is correct.
        /// </summary>
        /// <param name="applicationId">Id of application to filter by.</param>
        /// <param name="datasetId">Id of data to filter by.</param>
        /// <param name="dataId">Id of entity to get.</param>
        /// <returns>DataModel with application id, dataset id and id from parameters if such exists.</returns>
        public DataModel GetById(long applicationId, long datasetId, long dataId)
        {
            return model.FirstOrDefault(d => d.ApplicationId == applicationId &&
                                             d.DatasetId == datasetId &&
                                             d.Id == dataId);
        }
        /// <summary>
        /// This methods returns List of DataModels filtered by application and dataset ids.
        /// </summary>
        /// <param name="applicationId">Id of application to filter by.</param>
        /// <param name="datasetId">Id of dataset to filter by.</param>
        /// <returns>List of DataModels with application and dataset ids from parameters.</returns>
        public List<DataModel> GetAllByApplicationIdAndDatasetId(long applicationId, long datasetId)
        {
            return model.Where(d => d.ApplicationId == applicationId && d.DatasetId == datasetId).ToList();
        }
        /// <summary>
        /// This method sets data to a DataModel.
        /// </summary>
        /// <param name="model">Model to set name and data to.</param>
        /// <param name="data">Dictionary of the data.</param>
        /// <returns>Number of rows affected.</returns>
        public int SetData(DataModel model, Dictionary<string, List<object>> data)
        {
            string JsonData = JsonConvert.SerializeObject(data);
            model.Data = JsonData;
            return databaseContext.SaveChanges();
        }
        /// <summary>
        /// This method returns Lists of valid database DataModel ids grouped by datasets.
        /// </summary>
        /// <param name="applicationId">Id of application to filter by.</param>
        /// <returns>Dictionary containing dataset id as key and List of valid DataModel ids as value.</returns>
        public Dictionary<long, List<long>> GetAllReferencesIdsDictionary(long applicationId)
        {
            return model.Where(d => d.ApplicationId == applicationId)
                        .GroupBy(d => d.DatasetId)
                        .ToDictionary(key => key.Key, value => value.Select(d => d.Id).ToList());
        }




        public List<DataModel> GetAllByApplicationIdAndDatasetIdAndDataContentLike(long applicationId, long datasetId, string dataDictionaryLike)
        {
            return model.Where(d => d.ApplicationId == applicationId && d.DatasetId == datasetId && 
                               d.Data.Contains(dataDictionaryLike))
                        .ToList();
        }
    }
}