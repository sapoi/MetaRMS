using System;
using System.Collections.Generic;
using System.Security.Claims;
using Core.Helpers;
using Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Structures;

namespace Core.Controllers.Account.Settings
{
    [Route("api/account/settings/[controller]")]
    public class PasswordController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;
        public PasswordController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for password change.
        /// </summary>
        /// <param name="passwords">PasswordChangeStructure with passwords</param>
        /// <returns>Error of info message about action result</returns>
        /// <response code="200">If password successfully changed</response>
        /// <response code="400">If input passwords are not valid</response>
        /// <response code="401">If user is not authenticated</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult PasswordChange([FromBody] PasswordChangeStructure passwords)
        {
            // List of messages to return to the client
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization - none, because every logged user is authorized to change an own password.

            #region VALIDATIONS

            // All passwords must not be null or empty strings
            if (String.IsNullOrEmpty(passwords.OldPassword) ||
                String.IsNullOrEmpty(passwords.NewPassword) ||
                String.IsNullOrEmpty(passwords.NewPasswordCopy))
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  5001, 
                                                  new List<string>()));
                return BadRequest(messages);
            }

            // Both new passwords must be equal
            if (passwords.NewPassword != passwords.NewPasswordCopy)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  5002, 
                                                  new List<string>()));
                return BadRequest(messages);
            }

            // Old password must be correct
            if (authUserModel.PasswordHash != PasswordHelper.ComputeHash(authUserModel.PasswordSalt + passwords.OldPassword))
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  5003, 
                                                  new List<string>()));
                return BadRequest(messages);
            }

            // If passwords are required to be safer by application descriptor
            if (authUserModel.Application.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.PasswordAttribute.Safer == true)
            {
                var sharedValidationHelper = new SharedValidationHelper();
                if (!sharedValidationHelper.IsPasswordSafer(passwords.NewPassword))
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      5004, 
                                                      new List<string>()));
                    return BadRequest(messages);
                }
            }

            // If minimal password length is set
            var minPasswordLength = authUserModel.Application.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.PasswordAttribute.Min;
            if (minPasswordLength != null)
            {
                if (passwords.NewPassword.Length < minPasswordLength)
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                             5006, 
                                             new List<string>() {
                                                minPasswordLength.ToString(),
                                                passwords.NewPassword.Length.ToString()
                                             }));
                    return BadRequest(messages);
                }
            }

            #endregion

            // Setting new password
            var userRepository = new UserRepository(context);
            userRepository.SetPassword(authUserModel, passwords.NewPassword);
            messages.Add(new Message(MessageTypeEnum.Info, 
                                              5005, 
                                              new List<string>()));
            return Ok(messages);
        }
    }
}