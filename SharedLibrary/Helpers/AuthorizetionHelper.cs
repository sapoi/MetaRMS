using System.Collections.Generic;
using SharedLibrary.Enums;
using SharedLibrary.Models;

namespace SharedLibrary.Helpers
{
    public static class AuthorizationHelper
    {
        public static bool IsAuthorized(UserModel userModel, long datasetId, RightsEnum minimalRights)
        {
            return IsAuthorized(userModel.Rights.DataDictionary, datasetId, minimalRights);
        }
        public static bool IsAuthorized(Dictionary<long, RightsEnum> rightsDictionary, long datasetId, RightsEnum minimalRights)
        {
            return rightsDictionary[datasetId] >= minimalRights;
        }
    }
}