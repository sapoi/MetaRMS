using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Core.Helpers;
using Core.Repositories;
using SharedLibrary.Enums;
using SharedLibrary.Structures;
using SharedLibrary.Helpers;

namespace Core.Controllers.User
{
    [Route("api/user/[controller]")]
    public class ResetPasswordController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;

        public ResetPasswordController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint resetting user password.
        /// </summary>
        /// <param name="id">Id of user to reset the password</param>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If user pasword was sucessfully reset</response>
        /// <response code="400">If id is not valid</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to reset user passwords</response>
        [Authorize]
        [HttpPost]
        [Route("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public IActionResult ResetPasswordById(long id)
        {
            // List of messages to return to the client
            var messages = new List<Message>();
            
            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization
            if (!AuthorizationHelper.IsAuthorized(authUserModel, (long)SystemDatasetsEnum.Rights, RightsEnum.CRU))
                return Forbid();

            // Get data from database
            var userRepository = new UserRepository(context);
            var userModel = userRepository.GetById(authUserModel.ApplicationId, id);
            if (userModel == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  3004, 
                                                  new List<string>(){ authUserModel.Application.LoginApplicationName,
                                                                      id.ToString()
                                                                    }));
                return BadRequest(messages);
            }
            
            // Reset password
            userRepository.ResetPassword(userModel);
            messages.Add(new Message(MessageTypeEnum.Info, 
                                              3008, 
                                              new List<string>(){ userModel.GetUsername() }));
            return Ok(messages);
        }
    }
}