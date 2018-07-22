using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RazorWebApp.Pages.Account;

class AuthorizationHelper
{
    public static AccessToken GetToken(PageModel model)
    {
        var cookieData = model.HttpContext.Session.GetString("sessionJWT");
        if (cookieData == null)
            return null;
        return JsonConvert.DeserializeObject<AccessToken>(cookieData);
    }
    public static string GetAppNameFromToken(AccessToken token)
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenS = handler.ReadToken(token.token) as JwtSecurityToken;
        var claim =  tokenS.Claims.First(c => c.Type == "ApplicationName");
        if (claim == null)
            return null;
        return claim.Value;
        
    }
}