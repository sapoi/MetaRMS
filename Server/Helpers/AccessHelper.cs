using System.Linq;
using System.Security.Claims;
using Server;
using SharedLibrary.Models;

namespace Server.Helpers
{
    /// <summary>
    /// This class provides helper methods for server authentication and authorization.
    /// </summary>
    public class AccessHelper
    {
        /// <summary>
        /// 
        /// </summary>
        DatabaseContext _databaseContext;
        /// <summary>
        /// AccessHelper constructor.
        /// </summary>
        /// <param name="databaseContext"></param>
        public AccessHelper(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        /// <summary>
        /// This method finds user acccessing controller.
        /// </summary>
        /// <param name="user">ClaimsPrincipal item recieved in controller.</param>
        /// <returns>UserModel or null if no user war found.</returns>
        public UserModel GetRequestUserModel(ClaimsPrincipal user)
        {
            // get claimed named "UserId" from user's Claims
            var claim = user.Claims.First(c => c.Type == "UserId");
            // try to get and parse id from the claim
            if (claim == null)
                return null;
            long userId;
            if (!long.TryParse(claim.Value, out userId))
                return null;
            // get UserModel by id from claim or null if no user has such id
            return _databaseContext.UserDbSet.FirstOrDefault(u => u.Id == userId);
        }
    }
}