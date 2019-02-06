using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using Server.Repositories;
using Server.Helpers;

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
        public IActionResult Get()
        {
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            // Authorization - none - every logged user is authorized to read application descriptor
            // Get data from database
            var applicationRepository = new ApplicationRepository(_context);
            var applicationModel = applicationRepository.GetById(authUserModel.ApplicationId);
            if (applicationModel == null)
                return NotFound();
            return Ok(JsonConvert.DeserializeObject<ApplicationDescriptor>(applicationModel.ApplicationDescriptorJSON));





            // // get logged user's identity
            // var identity = HttpContext.User.Identity as ClaimsIdentity;
            // // if user is authenticated and JWT contains claim named LoginApplicationName
            // if (!identity.IsAuthenticated || identity.FindFirst("LoginApplicationName") == null)
            //     // user is not authorized to access application descriptor for application appName
            //     return Unauthorized();
            // // get value for LoginApplicationName claim
            // var appNameFromJWT = identity.FindFirst("LoginApplicationName").Value;
            // // try to look for application descriptor in database
            // var applicationRepository = new ApplicationRepository(_context);
            // var applicationModel = applicationRepository.GetByLoginApplicationName(appNameFromJWT);
            // // var query = (from p in _context.ApplicationDbSet
            // //              where p.LoginApplicationName == appNameFromJWT
            // //              select p.ApplicationDescriptorJSON).FirstOrDefault();
            // // application not found
            // if (applicationModel == null)
            //     return NotFound();
            // // application found successfully, return descriptor


            // return Ok(JsonConvert.DeserializeObject<ApplicationDescriptor>(applicationModel.ApplicationDescriptorJSON));

        }
    }
}