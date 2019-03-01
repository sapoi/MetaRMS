using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using System.Security.Claims;
using SharedLibrary.Enums;
using RazorWebApp.Repositories;
using RazorWebApp.Helpers;
using SharedLibrary.Structures;
using SharedLibrary.Helpers;

namespace RazorWebApp.Controllers.Rights
{
    [Route("api/rights/[controller]")]
    public class GetController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;

        public GetController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for getting all rights for application.
        /// </summary>
        /// <returns>List of RightsModel or messages about action result</returns>
        /// <response code="200">If rights successfully sent</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to read rights</response>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public IActionResult GetAll()
        {
            // List of messages to return to the client
            var messages = new List<Message>();
            
            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var userModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (userModel == null)
                return Unauthorized();

            // Authorization
            if (!AuthorizationHelper.IsAuthorized(userModel, (long)SystemDatasetsEnum.Rights, RightsEnum.R))
                return Forbid();

            // Get data from database
            var rightsRepository = new RightsRepository(context);
            var rightsModelList = rightsRepository.GetAllByApplicationId(userModel.ApplicationId);

            return Ok(rightsModelList);
        }

        /// <summary>
        /// API endpoint for getting rights by id.
        /// </summary>
        /// <returns>RightsModel or messages about action result</returns>
        /// <response code="200">If rights successfully sent</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to read rights</response>
        /// <response code="404">If id is not valid</response>
        [Authorize]
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult GetById(long id)
        {
            // List of messages
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization
            if (!AuthorizationHelper.IsAuthorized(authUserModel, (long)SystemDatasetsEnum.Rights, RightsEnum.R))
                return Forbid();

            // Get data from database
            var rightsRepository = new RightsRepository(context);
            var rightsModel = rightsRepository.GetById(authUserModel.ApplicationId, id);
            if (rightsModel == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  4006, 
                                                  new List<string>(){ id.ToString() }));
                return BadRequest(messages);
            }
            
            return Ok(rightsModel);
        }
    }
}