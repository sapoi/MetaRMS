using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;
using RazorWebApp.Repositories;
using RazorWebApp.Helpers;
using SharedLibrary.Structures;

namespace RazorWebApp.Controllers.User
{
    [Route("api/user/[controller]")]
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
        /// API endpoint for getting all users for application.
        /// </summary>
        /// <returns>List of UserModel or messages about action result</returns>
        /// <response code="200">If users successfully sent</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to read users</response>
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
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization
            if (!AuthorizationHelper.IsAuthorized(authUserModel, (long)SystemDatasetsEnum.Users, RightsEnum.R))
                return Forbid();

            // Get data from database
            var userRepository = new UserRepository(context);
            var userModelList = userRepository.GetAllByApplicationId(authUserModel.ApplicationId);

            // Prepare data for client
            DataHelper dataHelper = new DataHelper(context, authUserModel.Application, (long)SystemDatasetsEnum.Users);
            foreach (var row in userModelList)
                dataHelper.PrepareOneRowForClient(row);

            return Ok(userModelList);
        }

        /// <summary>
        /// API endpoint for getting user by id.
        /// </summary>
        /// <param name="id">Id of user to get</param>
        /// <returns>UserModel or messages about action result</returns>
        /// <response code="200">If user successfully sent</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to read users</response>
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
            var requestUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (requestUserModel == null)
                return Unauthorized();

            // Authorization
            if (!AuthorizationHelper.IsAuthorized(requestUserModel,(long)SystemDatasetsEnum.Users, RightsEnum.R))
                return Forbid();

            // Get data from database
            var userRepository = new UserRepository(context);
            var userModel = userRepository.GetById(id);
            if (userModel == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  3006, 
                                                  new List<string>(){ id.ToString() }));
                return BadRequest(messages);
            }

            return Ok(userModel);
        }
    }
}