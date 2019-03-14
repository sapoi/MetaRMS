using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Enums;
using SharedLibrary.Models;

namespace SharedLibrary.Helpers
{
    /// <summary>
    /// This helper contains static method regarding user authorization.
    /// </summary>
    public static class AuthorizationHelper
    {
        /// <summary>
        /// This method returns if the user is authorized for dataset from parameter to 
        /// at least minimal rights from parameter.
        /// </summary>
        /// <param name="userModel">Model of user to get the rights from</param>
        /// <param name="datasetId">Id of dataset to check authorization for</param>
        /// <param name="minimalRights">Minimal rights to be authorized</param>
        /// <returns>True if user is authorized, false otherwise.</returns>
        public static bool IsAuthorized(UserModel userModel, long datasetId, RightsEnum minimalRights)
        {
            return IsAuthorized(userModel.Rights.DataDictionary, datasetId, minimalRights);
        }
        /// <summary>
        /// This method returns if the user is authorized for dataset from parameter to 
        /// at least minimal rights from parameter.
        /// </summary>
        /// <param name="rightsDictionary">Rights dictionary for validation</param>
        /// <param name="datasetId">Id of dataset to check authorization for</param>
        /// <param name="minimalRights">Minimal rights to be authorized</param>
        /// <returns>True if rights is authorized, false otherwise.</returns>
        public static bool IsAuthorized(Dictionary<long, RightsEnum> rightsDictionary, long datasetId, RightsEnum minimalRights)
        {
            if (rightsDictionary.Keys.Contains(datasetId))
                return rightsDictionary[datasetId] >= minimalRights;
            Logger.LogToConsole($"Rights for dataset with id {datasetId} not in rightsDictionary.");
            return false;
        }
        /// <summary>
        /// This method returns rights for dataset from parameter. If rightsDictionary does not contain
        /// datasetId, null is returned.
        /// </summary>
        /// <param name="rightsDictionary">Rights dictionary to get the value from</param>
        /// <param name="datasetId">Id of dataset to get the rights for</param>
        /// <returns>Rights or null.</returns>
        public static RightsEnum? GetRights(Dictionary<long, RightsEnum> rightsDictionary, long datasetId)
        {
            if (rightsDictionary.Keys.Contains(datasetId))
                return rightsDictionary[datasetId];
            Logger.LogToConsole($"Rights for dataset with id {datasetId} not in rightsDictionary.");
            return null;
        }
    }
}