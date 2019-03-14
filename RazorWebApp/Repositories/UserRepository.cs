using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Models;

namespace RazorWebApp.Repositories
{
    /// <summary>
    /// Repository for UserModels.
    /// </summary>
    /// <typeparam name="UserModel">The type of database model</typeparam>
    public class UserRepository: BaseRepository<UserModel>
    {
        /// <summary>
        /// UserRepository constructor calling BaseRepository constructor.
        /// </summary>
        public UserRepository(DatabaseContext databaseContext):base(databaseContext, databaseContext.UserDbSet) { }
        /// <summary>
        /// The GetById method returns UserModel entity, if such found, with Rights and Application included.
        /// This method should not be used with id from user input, before previous validation.
        /// </summary>
        /// <param name="id">Id to get from the database.</param>
        /// <returns>UserModel with id from parameter if such user found.</returns>
        public new UserModel GetById(long id)
        {
            return model.Include(u => u.Rights)
                        .Include(u => u.Application)
                        .FirstOrDefault(u => u.Id == id);
        }
        /// <summary>
        /// The GetById method returns UserModel entity, if such found, with Rights and Application included.
        /// This method is used in controllers to get requested data by id only if also the application id is correct.
        /// </summary>
        /// <param name="applicationId">Application id thar the UserModel must have.</param>
        /// <param name="id">Id to get from the database.</param>
        /// <returns>UserModel with id and application id from parameters if such user found.</returns>
        public UserModel GetById(long applicationId, long id)
        {
            return model.Include(u => u.Rights)
                        .Include(u => u.Application)
                        .FirstOrDefault(u => u.ApplicationId == applicationId &&
                                        u.Id == id);
        }
        /// <summary>
        /// This method returns List of UserModels by application id and username.
        /// Only one such should exist.
        /// </summary>
        /// <param name="applicationId">Id of application to filter by.</param>
        /// <param name="username">Username to filter by.</param>
        /// <returns>List of UserModels with application id and username from parameters.</returns>
        public UserModel GetByApplicationIdAndUsername(long applicationId, string username)
        {
            var allApplicationUsers = this.GetAllByApplicationId(applicationId);
            return allApplicationUsers.Where(u => u.GetUsername() == username).FirstOrDefault();
        }
        /// <summary>
        /// This method returns all users for given application with Application and Rights included.
        /// </summary>
        /// <param name="applicationId">Id of application to filter by.</param>
        /// <returns>List of all UserModels of application from parameter.</returns>
        public List<UserModel> GetAllByApplicationId(long applicationId)
        {
            return model.Include(u => u.Rights)
                        .Include(u => u.Application)
                        .Where(u => u.ApplicationId == applicationId)
                        .ToList();
        }
        /// <summary>
        /// This method returns List of UserModels by rights id.
        /// </summary>
        /// <param name="rightsId">Id of rights to filter by.</param>
        /// <returns>List of UserModels with rights id from parameter.</returns>
        public List<UserModel> GetByRightsId(long rightsId)
        {
            return model.Include(u => u.Rights)
                        .Include(u => u.Application)
                        .Where(u => u.RightsId == rightsId)
                        .ToList();
        }
        /// <summary>
        /// This method sets password of UserModel from parameter to username form parameter.
        /// </summary>
        /// <param name="user">UserModel that should have the password set.</param>
        /// <param name="password">New password to be set.</param>
        /// <returns>Number of rows affected.</returns>
        public int SetPassword(UserModel user, string password)
        {
            user.Password = PasswordHelper.ComputeHash(password);
            return databaseContext.SaveChanges();
        }
        /// <summary>
        /// This method seth user password to default value (the same as the username).
        /// </summary>
        /// <param name="user">UserModel that should have the password set.</param>
        /// <returns>Number of rows affected.</returns>
        public int ResetPassword(UserModel user)
        {
            string password = user.GetUsername();
            return this.SetPassword(user, password);
        }
        /// <summary>
        /// This method sets rights id and data to a UserModel.
        /// </summary>
        /// <param name="user">UserModel to set rights id and data to.</param>
        /// <param name="rightsId">Id of rights to be set.</param>
        /// <param name="data">Dictionary of the data.</param>
        /// <returns>Number of rows affected.</returns>
        public int SetRightsIdAndData(UserModel user, long rightsId, Dictionary<string, List<object>> data)
        {
            user.Data = JsonConvert.SerializeObject(data);
            user.RightsId = rightsId;
            return databaseContext.SaveChanges();
        }
        /// <summary>
        /// This method returns list of valid database UserModel ids.
        /// </summary>
        /// <param name="applicationId">Id of application to filter by.</param>
        /// <returns>KeyValuePair containing SystemDatasetsEnum.Users as key and List of valid UserModel ids as value.</returns>
        public KeyValuePair<long, List<long>> GetAllReferencesIdsDictionary(long applicationId)
        {
            return new KeyValuePair<long, List<long>>(
                (long)SystemDatasetsEnum.Users, 
                model.Where(u => u.ApplicationId == applicationId).Select(u => u.Id).ToList()
            );
        }
        /// <summary>
        /// This method returns list of UserModels for application from parametres, that also have dataDictionaryLike 
        /// value from parametres in Data attribute. This is used when looking for references.
        /// </summary>
        /// <param name="applicationId">Id of application to filter by.</param>
        /// <param name="dataDictionaryLike">Value to look for in Data attribute.</param>
        /// <returns>List of UserModels with Data containing value from parameter dataDictionaryLike.</returns>
        public List<UserModel> GetAllByApplicationIdAndDataContentLike(long applicationId, string dataDictionaryLike)
        {
            return model.Where(u => u.ApplicationId == applicationId && u.Data.Contains(dataDictionaryLike))
                        .ToList();
        }
    }
}