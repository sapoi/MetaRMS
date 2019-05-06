using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Helpers;
using System.Security.Claims;
using Core.Helpers;
using Core.Repositories;
using SharedLibrary.Enums;
using SharedLibrary.Structures;

namespace Core.Controllers.User
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
                dataHelper.PrepareOneRecordForClient(row);

            return Ok(userModelList);
        }

        /// <summary>
        /// API endpoint for getting user by id.
        /// </summary>
        /// <param name="id">Id of user to get</param>
        /// <returns>UserModel or messages about action result</returns>
        /// <response code="200">If user successfully sent</response>
        /// <response code="400">If id is not valid</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to read users</response>
        [Authorize]
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
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
            if (!AuthorizationHelper.IsAuthorized(authUserModel,(long)SystemDatasetsEnum.Users, RightsEnum.R))
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

            // Prepare data for client
            DataHelper dataHelper = new DataHelper(context, authUserModel.Application, (long)SystemDatasetsEnum.Users);
            dataHelper.PrepareOneRecordForClient(userModel);

            return Ok(userModel);
        }
    }
}