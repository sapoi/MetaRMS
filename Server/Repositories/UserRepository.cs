using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedLibrary.Helpers;
using SharedLibrary.Models;

namespace Server.Repositories
{
    public class UserRepository: BaseRepository<UserModel>
    {
        public UserRepository(DatabaseContext databaseContext):base(databaseContext, databaseContext.UserDbSet) { }
        public UserModel GetById(long id)
        {
            return _model.Include(u => u.Rights)
                         .Include(u => u.Application)
                         .FirstOrDefault(d => d.Id == id);
        }
        public UserModel GetByApplicationIdAndUsername(long applicationId, string username)
        {
            var allApplicationUsers = this.GetAllByApplicationId(applicationId);
            return allApplicationUsers.Where(u => u.GetUsername() == username).FirstOrDefault();
            // return _model.Include(u => u.Application)
            //              .Where(u => (u.Application.LoginApplicationName == loginApplicationName && 
            //                           u.GetUsername() == username))
            //              .FirstOrDefault();
        }
        public List<UserModel> GetAllByApplicationId(long applicationId)
        {
            return _model.Where(u => u.ApplicationId == applicationId).Include(u => u.Rights).ToList();
        }
        public List<UserModel> GetByRightsId(long rightsId)
        {
            return _model.Where(u => u.RightsId == rightsId).ToList();
        }
        public int SetPassword(UserModel user, string password)
        {
            user.Password = PasswordHelper.ComputeHash(password);
            return _databaseContext.SaveChanges();
        }
        public int ResetPassword(UserModel user)
        {
            string password = user.GetUsername();
            return this.SetPassword(user, password);
        }
        public int SetRightsIdAndData(UserModel user, long rightsId, Dictionary<string, List<object>> data)
        {
            user.Data = JsonConvert.SerializeObject(data);
            user.RightsId = rightsId;
            return _databaseContext.SaveChanges();
        }
    }
}