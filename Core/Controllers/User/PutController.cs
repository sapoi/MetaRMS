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
    public class PutController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;

        public PutController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for putting user.
        /// </summary>
        /// <param name="fromBodyUserModel">Changed UserModel</param>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If user successfully putted</response>
        /// <response code="400">If input is not valid</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to put users</response>
        [Authorize]
        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public IActionResult Put([FromBody] UserModel fromBodyUserModel)
        {
            // List of messages to return to the client
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization
            if (!AuthorizationHelper.IsAuthorized(authUserModel, (long)SystemDatasetsEnum.Users, RightsEnum.CRU))
                return Forbid();

            #region VALIDATIONS

            // Received user ApplicationId must be the same as of authorized user
            var sharedValidationHelper = new SharedValidationHelper();
            messages = sharedValidationHelper.ValidateApplicationId(fromBodyUserModel.ApplicationId, authUserModel.ApplicationId);
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
            if (string.IsNullOrEmpty(fromBodyUserModel.GetUsername()))
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
            messages = sharedValidationHelper.ValidateDataByApplicationDescriptor(authUserModel.Application.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor, 
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