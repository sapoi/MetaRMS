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
using System.Collections.Generic;
using SharedLibrary.Structures;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    public class ApplicationDescriptorController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;
        public ApplicationDescriptorController(DatabaseContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// API endpoint for getting application descriptor for authenticated user.
        /// </summary>
        /// <returns>ApplicationDescriptor</returns>
        /// <response code="200">Returns application descriptor</response>
        /// <response code="401">If user is not authenticated</response>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult Get()
        {
            // List of messages to return to the client
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization - none, because every logged user is authorized to read an own application descriptor.

            return Ok(authUserModel.Application.ApplicationDescriptor);
        }
    }
}