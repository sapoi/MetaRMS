using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SharedLibrary.Models;
using System.Linq;
using SharedLibrary.Helpers;
using System.Security.Claims;
using Core.Helpers;
using Core.Repositories;
using SharedLibrary.Enums;
using SharedLibrary.Structures;

namespace Core.Controllers.User
{
    [Route("api/user/[controller]")]
    public class CreateController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;

        public CreateController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for creating user.
        /// </summary>
        /// <param name="fromBodyUserModel">New UserModel</param>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If user successfully created</response>
        /// <response code="400">If input is not valid</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to create users</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public IActionResult Create([FromBody] UserModel fromBodyUserModel)
        {
            // List of messages to return to the client
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization
            if (!AuthorizationHelper.IsAuthorized(authUserModel, (long)SystemDatasetsEnum.Users, RightsEnum.CR))
                return Forbid();

            #region VALIDATIONS

            // New user ApplicationId must be the same as of authorized user
            var sharedValidationHelper = new SharedValidationHelper();
            messages = sharedValidationHelper.ValidateApplicationId(fromBodyUserModel.ApplicationId, authUserModel.ApplicationId);
            if (messages.Count != 0)
                return BadRequest(messages);
            fromBodyUserModel.Application = authUserModel.Application;
            
            // New username must be nonempty
            if (String.IsNullOrEmpty(fromBodyUserModel.GetUsername()))
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  3001, 
                                                  new List<string>()));
                return BadRequest(messages);
            }

            // New username must be unique
            var userRepository = new UserRepository(context);
            var sameNameUser = userRepository.GetByApplicationIdAndUsername(authUserModel.ApplicationId, fromBodyUserModel.GetUsername());
            if (sameNameUser != null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  3002, 
                                                  new List<string>(){ fromBodyUserModel.GetUsername() }));
                return BadRequest(messages);
            }

            // Input data validations
            var validReferencesIdsDictionary = controllerHelper.GetAllReferencesIdsDictionary(authUserModel.Application);
            messages = sharedValidationHelper.ValidateDataByApplicationDescriptor(authUserModel.Application.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor, 
                                                                               fromBodyUserModel.DataDictionary, 
                                                                               validReferencesIdsDictionary);
            if (messages.Count != 0)
                return BadRequest(messages);

            #endregion

            // Reset password to default
            userRepository.ResetPassword(fromBodyUserModel);

            // Set defalut language from application
            fromBodyUserModel.Language = fromBodyUserModel.Application.ApplicationDescriptor.DefaultLanguage;

            userRepository.Add(fromBodyUserModel);
            messages.Add(new Message(MessageTypeEnum.Info, 
                                              3003, 
                                              new List<string>(){ fromBodyUserModel.GetUsername() }));
            return Ok(messages);
        }
    }
}