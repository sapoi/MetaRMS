using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RazorWebApp.Pages.Account;
using static SharedLibrary.Structures.JWTToken;

namespace RazorWebApp.Helpers
{
    class AuthorizationHelper
    {
        public static AccessToken GetTokenFromPageModel(PageModel model)
        {
            var cookieData = model.HttpContext.Session.GetString("sessionJWT");
            if (cookieData == null)
                return null;
            return JsonConvert.DeserializeObject<AccessToken>(cookieData);
        }
    }
}