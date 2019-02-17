using static SharedLibrary.Structures.JWTToken;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

public class TokenHelper
{
    AccessToken accessToken;
    public TokenHelper(AccessToken token)
    {
        accessToken = token;
    }
    // public string GetLoginApplicationName() not in the claims anymore
    // {
    //     var handler = new JwtSecurityTokenHandler();
    //     var token = handler.ReadToken(_accessToken.Value) as JwtSecurityToken;
    //     var claim =  token.Claims.First(c => c.Type == "LoginApplicationName");
    //     if (claim == null)
    //         return null;
    //     return claim.Value;
        
    // }
    public long? GetUserId()
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(accessToken.Value) as JwtSecurityToken;
        var claim =  token.Claims.First(c => c.Type == "UserId");
        if (claim == null)
            return null;
        long id;
        if (!long.TryParse(claim.Value, out id))
            return null;
        return id;
    }
    public long? GetApplicationId()
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(accessToken.Value) as JwtSecurityToken;
        var claim =  token.Claims.First(c => c.Type == "ApplicationId");
        if (claim == null)
            return null;
        long id;
        if (!long.TryParse(claim.Value, out id))
            return null;
        return id;
    }
}