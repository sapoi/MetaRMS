using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Helpers;
using Server.Repositories;
using Server.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;
using SharedLibrary.Structures;

namespace Server.Controllers.User
{
    [Route("api/user/[controller]")]
    public class PatchController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;

        public PatchController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for patching user.
        /// </summary>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If user successfully patched</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to patch users</response>
        /// <response code="404">If input is not valid</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult Patch([FromBody] UserModel fromBodyUserModel)
        {
            // List of messages to return to the client
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization
            if (!controllerHelper.Authorize(authUserModel, (long)SystemDatasetsEnum.Users, RightsEnum.RU))
                return Forbid();

            #region VALIDATIONS

            // Received user ApplicationId must be the same as of authorized user
            var validationsHelper = new ValidationsHelper();
            messages = validationsHelper.ValidateApplicationId(fromBodyUserModel.ApplicationId, authUserModel.ApplicationId);
            if (messages.Count != 0)
                return BadRequest(messages);
            fromBodyUserModel.Application = authUserModel.Application;
            
            // User must already exist in the database
            var userRepository = new UserRepository(context);
            var userModel = userRepository.GetById(authUserModel.ApplicationId, fromBodyUserModel.Id);
            if (userModel == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  3004, 
                                                  new List<string>(){ fromBodyUserModel.Application.LoginApplicationName,
                                                                      fromBodyUserModel.Id.ToString()
                                                                    }));
                Logger.LogMessagesToConsole(messages);
                return BadRequest(messages);
            }

            // New username must be nonempty
            if (fromBodyUserModel.GetUsername() == null || fromBodyUserModel.GetUsername() == "")
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  3001, 
                                                  new List<string>()));
                return BadRequest(messages);
            }
            
            // If the username was changed, the new one must be unique
            if (userModel.GetUsername() != fromBodyUserModel.GetUsername()) 
            {
                var sameNameUser = userRepository.GetByApplicationIdAndUsername(authUserModel.ApplicationId, fromBodyUserModel.GetUsername());
                if (sameNameUser != null)
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      3002, 
                                                      new List<string>(){ fromBodyUserModel.GetUsername() }));
                    return BadRequest(messages);
                }
            }

            // Input data validations
            var validReferencesIdsDictionary = controllerHelper.GetAllReferencesIdsDictionary(authUserModel.Application);
            messages = validationsHelper.ValidateDataByApplicationDescriptor(authUserModel.Application.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor, 
                                                                               fromBodyUserModel.DataDictionary, 
                                                                               validReferencesIdsDictionary);
            if (messages.Count != 0)
                return BadRequest(messages);

            #endregion

            userRepository.SetRightsIdAndData(userModel, fromBodyUserModel.RightsId, fromBodyUserModel.DataDictionary);
            messages.Add(new Message(MessageTypeEnum.Info, 
                                              3007, 
                                              new List<string>(){ fromBodyUserModel.GetUsername() }));
            return Ok(messages);
        }
    }
}