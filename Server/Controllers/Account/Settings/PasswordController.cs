using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Helpers;
using Server.Repositories;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Structures;

namespace Server.Controllers.Account.Settings
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
        /// <returns>Error of info message about action result</returns>
        /// <response code="200">If password successfully changed</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="404">If input passwords are not valid</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
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
            if (passwords.OldPassword == null || passwords.OldPassword == "" ||
                passwords.NewPassword == null || passwords.NewPassword == "" ||
                passwords.NewPasswordCopy == null || passwords.NewPasswordCopy == "")
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
            if (authUserModel.Password != PasswordHelper.ComputeHash(passwords.OldPassword))
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  5003, 
                                                  new List<string>()));
                return BadRequest(messages);
            }

            // If passwords are required to be safer by application descriptor
            if (authUserModel.Application.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.PasswordAttribute.Safer == true)
            {
                var validationsHelper = new ValidationsHelper();
                if (!validationsHelper.IsPasswordSafer(passwords.NewPassword))
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      5004, 
                                                      new List<string>()));
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