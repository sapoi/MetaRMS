using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RazorWebApp.Repositories;
using RazorWebApp.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;
using System.Collections.Generic;
using SharedLibrary.Structures;
using SharedLibrary.Helpers;

namespace RazorWebApp.Controllers.User
{
    [Route("api/user/[controller]")]
    public class DeleteController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;

        public DeleteController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for deleting users.
        /// </summary>
        /// <param name="id">Id of user to delete</param>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If user successfully deleted</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to delete users</response>
        /// <response code="404">If input is not valid or user can not be deleted</response>
        [Authorize]
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult DeleteById(long id)
        {
            // List of messages
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization
            if (!AuthorizationHelper.IsAuthorized(authUserModel, (long)SystemDatasetsEnum.Users, RightsEnum.CRUD))
                return Forbid();

            #region VALIDATIONS

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
                Logger.LogMessagesToConsole(messages);
                return BadRequest(messages);
            }
            // Can not delete last user of the application
            if (userRepository.GetAllByApplicationId(authUserModel.ApplicationId).Count <= 1)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  3013, 
                                                  new List<string>()));
                return BadRequest(messages);
            }

            // Delete validity check
            using (var transaction = context.Database.BeginTransaction())
            {
                // Set to delete or remove all references from data referencing userModel to delete
                if (controllerHelper.IfCanBeDeletedPerformDeleteActions(authUserModel, userModel))
                {
                    // Remove model itself
                    userRepository.Remove(userModel);
                    // Save all performed changes into the database
                    transaction.Commit();
                    messages.Add(new Message(MessageTypeEnum.Info, 
                                              3005, 
                                              new List<string>(){ userModel.GetUsername() }));
                    return Ok(messages);
                }
                // UserModel to delete or other model that should be deleted can not be deleted
                transaction.Rollback();
                messages.Add(new Message(MessageTypeEnum.Error, 
                                              3014, 
                                              new List<string>()));
                return BadRequest(messages);
            }

            #endregion
        }
    }
}