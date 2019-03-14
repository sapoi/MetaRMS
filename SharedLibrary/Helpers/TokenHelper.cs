using static SharedLibrary.Structures.JWTToken;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using SharedLibrary.Structures;

namespace SharedLibrary.Helpers
{
    /// <summary>
    /// Token helper is used to get user and application id from access token
    /// </summary>
    public class TokenHelper
    {
        /// <summary>
        /// AccessToken to get the data from
        /// </summary>
        JWTToken accessToken;
        public TokenHelper(JWTToken token)
        {
            accessToken = token;
        }
        /// <summary>
        /// This method returns user id from token if the token contains one.
        /// </summary>
        /// <returns>User id if found in token</returns>
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
        /// <summary>
        ///  This method returns application id from token if the token contains one.
        /// </summary>
        /// <returns>Application id if found in token</returns>
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
}