using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    public class ApplicationDescriptorController : Controller
    {
        private readonly DatabaseContext _context;

        public ApplicationDescriptorController(DatabaseContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(200)] // returns 200 if descriptor found
        [ProducesResponseType(401)] // returns 401 if user is not authorized to get descriptor for chosen appName
        [ProducesResponseType(404)] // returns 404 if descriptor not found
        // get application descriptor
        public IActionResult GetByName()
        {
            // get logged user's identity
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            // if user is authenticated and JWT contains claim named ApplicationName
            if (!identity.IsAuthenticated || identity.FindFirst("ApplicationName") == null)
                // user is not authorized to access application descriptor for application appName
                return Unauthorized();
            // get value for ApplicationName claim
            var appNameFromJWT = identity.FindFirst("ApplicationName").Value;
            // try to look for application descriptor in database
            var query = (from p in _context.ApplicationDbSet
                         where p.LoginApplicationName == appNameFromJWT
                         select p.ApplicationDescriptorJSON).FirstOrDefault();
            // application descriptor not found
            if (query == null)
                return NotFound();
            // application descriptor found successfully


            return Ok(JsonConvert.DeserializeObject<ApplicationDescriptor>(query));

        }
    }
}