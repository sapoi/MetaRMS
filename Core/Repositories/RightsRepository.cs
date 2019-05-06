using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SharedLibrary.Enums;
using SharedLibrary.Models;

namespace Core.Repositories
{
    /// <summary>
    /// Repository for RightsModels.
    /// </summary>
    public class RightsRepository: BaseRepository<RightsModel>
    {
        /// <summary>
        /// RightsRepository constructor calling BaseRepository constructor.
        /// </summary>
        public RightsRepository(DatabaseContext databaseContext):base(databaseContext, databaseContext.RightsDbSet) { }
        /// <summary>
        /// The GetById method returns RightsModel entity, if such found.
        /// This method is used in controllers to get requested data by id only if also the application id is correct.
        /// </summary>
        /// <param name="applicationId">Application id thar the RightsModel must have.</param>
        /// <param name="id">Id to get from the database.</param>
        /// <returns>RightsModel with id and application id from parameters if such rights found.</returns>
        public RightsModel GetById(long applicationId, long id)
        {
            return model.FirstOrDefault(r => r.ApplicationId == applicationId &&
                                             r.Id == id);
        }
        /// <summary>
        /// This method returns List of RightsModels by application id and rights name.
        /// Only one such should exist.
        /// </summary>
        /// <param name="applicationId">Id of application to filter by.</param>
        /// <param name="name">Name of the rights.</param>
        /// <returns>List of RightsModels.</returns>
        public List<RightsModel> GetByApplicationIdAndName(long applicationId, string name)
        {
            return model.Where(r => r.ApplicationId == applicationId && 
                                    r.Name == name)
                        .ToList();
        }
        /// <summary>
        /// This method returns List of RightsModels by application id.
        /// </summary>
        /// <param name="applicationId">Id of application to filter by.</param>
        /// <returns>List of all RightsModels for the application.</returns>
        public List<RightsModel> GetAllByApplicationId(long applicationId)
        {
            return model.Where(r => r.ApplicationId == applicationId).ToList();
        }
        /// <summary>
        /// This method sets name and rights data to a RightsModel.
        /// </summary>
        /// <param name="model">RightsModel to set name and data to.</param>
        /// <param name="name">Name of the rights.</param>
        /// <param name="data">Dictionary of rights data.</param>
        /// <returns>Number of rows affected.</returns>
        public int SetNameAndData(RightsModel model, string name, Dictionary<long, RightsEnum> data)
        {
            model.Name = name;
            model.Data = JsonConvert.SerializeObject(data);
            return databaseContext.SaveChanges();
        }
    }
}