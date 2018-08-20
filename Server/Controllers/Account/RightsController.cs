using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using System.Collections.Generic;
using SharedLibrary.Enums;

namespace Server.Controllers.Account
{
    [Route("api/account/[controller]")]
    public class RightsController : Controller
    {
        private readonly DatabaseContext _context;

        public RightsController(DatabaseContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        //[ProducesResponseType(200)] // returns 200 if descriptor found
        //[ProducesResponseType(401)] // returns 401 if user is not authorized to get descriptor for chosen appName
        //[ProducesResponseType(404)] // returns 404 if descriptor not found
        // get application descriptor
        public IActionResult GetByIdFromToken()
        {
            // get logged user's identity
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            // if user is authenticated and JWT contains claim named ApplicationName
            if (identity == null || !identity.IsAuthenticated 
                                 || identity.FindFirst("ApplicationName") == null
                                 || identity.FindFirst("UserId") == null)
                // user is not authorized to access application descriptor for application appName
                return Unauthorized();
            // get user id for UserId claim
            var userIdString = identity.FindFirst("UserId").Value;
            long userId;
            if (!long.TryParse(userIdString, out userId))
                return BadRequest("UserId claim could not be parsed");
            // try to look for user in DB
            var user = (from u in _context.UserDbSet
                        where u.Id == userId
                        select u).FirstOrDefault();
            if (user == null)
                return BadRequest("No such user with user id " + userId);
            var rights = (from r in _context.RightsDbSet
                          where r.Id == user.RightsId
                          select r).FirstOrDefault();
            if (rights == null)
                return BadRequest("No such rights with id" + user.RightsId);
            if (rights.Data == null || rights.Data == "")
                return BadRequest("Rights with unfilled data.");
            try
            {
                return Ok(JsonConvert.DeserializeObject<Dictionary<long, RightsEnum>>(rights.Data));
            }
            catch
            {
                return BadRequest("Malformed rights data.");
            }
        }

    }
}
