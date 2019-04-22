using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using Core.Repositories;
using System.Collections.Generic;
using Core.Helpers;
using SharedLibrary.Structures;
namespace Core.Controllers.Account
{
    [Route("api/account/[controller]")]
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