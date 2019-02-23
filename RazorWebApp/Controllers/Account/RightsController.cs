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
using RazorWebApp.Repositories;
using RazorWebApp.Helpers;
using Server;

namespace RazorWebApp.Controllers.Account
{
    [Route("api/account/[controller]")]
    public class RightsController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;
        public RightsController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for getting rights for authenticated user.
        /// </summary>
        /// <returns>User RightsModel</returns>
        /// <response code="200">Returns user rights</response>
        /// <response code="401">If user is not authenticated</response>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetByIdFromToken()
        {
            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization - none, because every logged user is authorized to read own rights.
            
            return Ok(authUserModel.Rights);
        }

    }
}
