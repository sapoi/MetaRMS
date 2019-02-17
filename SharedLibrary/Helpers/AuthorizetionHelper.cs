// using SharedLibrary.Enums;
// using SharedLibrary.Models;

// namespace SharedLibrary.Helpers
// {
//     public class AuthorizationHelper
//     {
//         public bool IsAuthorized(UserModel userModel, long datasetId, RightsEnum minimalRights)
//         {
//             return (RightsEnum)userModel.Rights.DataDictionary[datasetId.ToString()] >= minimalRights;
//         }
//     }
// }