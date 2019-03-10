using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using RazorWebApp.Repositories;
using RazorWebApp.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;
using SharedLibrary.Structures;
using SharedLibrary.Helpers;

namespace RazorWebApp.Controllers.Rights
{
    [Route("api/rights/[controller]")]
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
        /// API endpoint for deleting rights.
        /// </summary>
        /// <param name="id">Id of rights to delete</param>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If rights successfully deleted</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to delete rights</response>
        /// <response code="404">If input is not valid</response>
        [Authorize]
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult DeleteById(long id)
        {
            // List of messages to return to the client
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization
            if (!AuthorizationHelper.IsAuthorized(authUserModel, (long)SystemDatasetsEnum.Rights, RightsEnum.CRUD))
                return Forbid();
            
            #region VALIDATIONS

            // Rights must already exist in the database
            var rightsRepository = new RightsRepository(context);
            var rightsModel = rightsRepository.GetById(authUserModel.ApplicationId, id);
            if (rightsModel == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                         4003, 
                                         new List<string>(){ rightsModel.Application.LoginApplicationName,
                                                             rightsModel.Id.ToString()
                                                            }));
                Logger.LogMessagesToConsole(messages);
                return BadRequest(messages);
            }
            
            // Check if no users are using rights to delete
            var userRepository = new UserRepository(context);
            List<UserModel> users = userRepository.GetByRightsId(rightsModel.Id);
            if (users.Count > 0)
            {
                string usernames = users[0].GetUsername();
                for (int i = 1; i < users.Count; i++)
                    usernames += ", " + users[i].GetUsername();
                messages.Add(new Message(MessageTypeEnum.Error, 
                                         4003, 
                                         new List<string>(){ rightsModel.Name,
                                                             usernames
                                                            }));
                return BadRequest(messages);                                            
            }

            #endregion

            // Remove rights
            rightsRepository.Remove(rightsModel);
            messages.Add(new Message(MessageTypeEnum.Info, 
                                     4005, 
                                     new List<string>(){ rightsModel.Name }));
            return Ok(messages);
        }
    }
}