using static SharedLibrary.Structures.JWTToken;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

public class TokenHelper
{
    AccessToken _accessToken;
    public TokenHelper(AccessToken token)
    {
        _accessToken = token;
    }
    public string GetAppName()
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(_accessToken.Value) as JwtSecurityToken;
        var claim =  token.Claims.First(c => c.Type == "ApplicationName");
        if (claim == null)
            return null;
        return claim.Value;
        
    }
    public long? GetUserId()
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(_accessToken.Value) as JwtSecurityToken;
        var claim =  token.Claims.First(c => c.Type == "UserId");
        if (claim == null)
            return null;
        long id;
        if (!long.TryParse(claim.Value, out id))
            return null;
        return id;
    }
}