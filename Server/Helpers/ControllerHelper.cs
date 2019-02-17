using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Server.Repositories;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Models;

namespace Server.Helpers
{
    public class ControllerHelper
    {
        DatabaseContext context;
        public ControllerHelper(DatabaseContext context)
        {
            this.context = context;
        }
        public UserModel Authenticate(ClaimsIdentity identity)
        {
            // if user is authenticated and JWT contains claim named LoginApplicationName
            if (identity == null || !identity.IsAuthenticated 
                                 || identity.FindFirst("UserId") == null)
                // user is not authorized to access application descriptor for application appName
            {
                //TODO
                Logger.LogToConsole("");
                return null;
            }
            // get user id for UserId claim
            long userId;
            if (!long.TryParse(identity.FindFirst("UserId").Value, out userId))
            {
                Logger.LogToConsole($"UserId claim with value \"{identity.FindFirst("UserId").Value}\" could not be parsed.");
                return null;
            }
            // try to look for user in DB
            var userRepository = new UserRepository(context);
            var userModel = userRepository.GetById(userId);
            if (userModel == null)
            {
                Logger.LogToConsole($"No user with id \"{userId}\" found.");
                return null;
            }
            Logger.LogToConsole($"User with id \"{userModel.Id}\" successfully authenticated.");
            return userModel;
        }
        public bool Authorize(UserModel userModel, long datasetId, RightsEnum minimalRights)
        {
            return (RightsEnum)userModel.Rights.DataDictionary[datasetId] >= minimalRights;
        }
        public Dictionary<string, List<long>> GetAllReferencesIdsDictionary(ApplicationModel applicationModel)
        {
            var stringKeyDictionary = new Dictionary<string, List<long>>();
            var dataRepository = new DataRepository(context);
            var longKeyDictionary = dataRepository.GetAllReferencesIdsDictionary(applicationModel.Id);
            foreach (var item in longKeyDictionary)
            {
                string key = applicationModel.ApplicationDescriptor.Datasets.First(d => d.Id == item.Key).Name;
                stringKeyDictionary.Add(key, item.Value);
            }
            var userRepository = new UserRepository(context);
            var longKeyUserReferences = userRepository.GetAllReferencesIdsDictionary(applicationModel.Id);
            stringKeyDictionary.Add(applicationModel.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name,
                                    longKeyUserReferences.Value);
            return stringKeyDictionary;
        }
    }
}