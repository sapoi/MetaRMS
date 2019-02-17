// using System.Collections.Generic;
// using System.IdentityModel.Tokens.Jwt;
// using System.Linq;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using Newtonsoft.Json;
// using RazorWebApp.Pages.Account;
// using SharedLibrary.Enums;
// using SharedLibrary.Helpers;
// using static SharedLibrary.Structures.JWTToken;

// namespace RazorWebApp.Helpers
// {
//     class AuthorizationHelper
//     {
//         // public static AccessToken GetTokenFromPageModel(PageModel model)
//         // {
//         //     // Get session data from page model
//         //     var sessionData = model.HttpContext.Session.GetString("sessionJWT");
//         //     if (sessionData == null)
//         //     {
//         //         Logger.LogToConsole($"New request from address {model.Request.Host.Host} without token.");
//         //         return null;
//         //     }
//         //     // Parse token from session data
//         //     AccessToken token;
//         //     try
//         //     {
//         //         token = JsonConvert.DeserializeObject<AccessToken>(sessionData);
//         //     }
//         //     catch
//         //     {
//         //         Logger.LogToConsole($"New request from address {model.Request.Host.Host} with cookie data {sessionData} without token.");
//         //         return null;
//         //     }
//         //     // Get application id and user id from token
//         //     TokenHelper tokenHelper = new TokenHelper(token);
//         //     // Application id
//         //     var applicationId = tokenHelper.GetApplicationId();
//         //     // If token did not contain application id
//         //     if (!applicationId.HasValue)
//         //     {
//         //         Logger.LogToConsole($"There was no application id in token {token.Value}.");
//         //         return null;
//         //     }
//         //     token.ApplicationId = (long)applicationId;
//         //     // User id
//         //     var userId = tokenHelper.GetUserId();
//         //     // If token did not contain user id
//         //     if (!userId.HasValue)
//         //     {
//         //         Logger.LogToConsole($"There was no user id in token {token.Value}.");
//         //         return null;
//         //     }
//         //     token.UserId = (long)userId;

//         //     // Return valid token
//         //     return token;
//         // }







//         public static RightsEnum? GetRights(Dictionary<long, RightsEnum> rights, long datasetId)
//         {
//             if (rights.Keys.Contains(datasetId))
//                 return rights[datasetId];
//             return null;
//         }
//     }
// }