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
        DatabaseContext _context;
        public ControllerHelper(DatabaseContext context)
        {
            _context = context;
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
            var userRepository = new UserRepository(_context);
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
            return (RightsEnum)userModel.Rights.DataDictionary[datasetId.ToString()] >= minimalRights;
        }
    }
}