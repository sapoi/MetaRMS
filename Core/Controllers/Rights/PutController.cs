using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using System.Security.Claims;
using SharedLibrary.Enums;
using System.Collections.Generic;
using Core.Helpers;
using Core.Repositories;
using SharedLibrary.Structures;
using SharedLibrary.Helpers;
using System.Linq;

namespace Core.Controllers.Rights
{
    [Route("api/rights/[controller]")]
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
        /// API endpoint for putting rights.
        /// </summary>
        /// <param name="fromBodyRightsModel">Changed RightsModel</param>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If rights successfully putted</response>
        /// <response code="400">If input is not valid</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to put rights</response>
        [Authorize]
        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public IActionResult Put([FromBody] RightsModel fromBodyRightsModel)
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

            #region VALIDATIONS

            // Received rights ApplicationId must be the same as of authorized user
            var sharedValidationHelper = new SharedValidationHelper();
            messages = sharedValidationHelper.ValidateApplicationId(fromBodyRightsModel.ApplicationId, authUserModel.ApplicationId);
            if (messages.Count != 0)
                return BadRequest(messages);
            fromBodyRightsModel.Application = authUserModel.Application;

            // Rights must already exist in the database
            var rightsRepository = new RightsRepository(context);
            var rightsModel = rightsRepository.GetById(authUserModel.ApplicationId, fromBodyRightsModel.Id);
            if (rightsModel == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  4003, 
                                                  new List<string>(){ fromBodyRightsModel.Application.LoginApplicationName,
                                                                      fromBodyRightsModel.Id.ToString()
                                                                    }));
                Logger.LogMessagesToConsole(messages);
                return BadRequest(messages);
            }
            
            // If the rights name was changed, the new one must be unique
            if (rightsModel.Name != fromBodyRightsModel.Name) 
            {
                var sameNameRights = rightsRepository.GetByApplicationIdAndName(authUserModel.ApplicationId, fromBodyRightsModel.Name);
                if (sameNameRights.Count() > 0)
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                    4001, 
                                                    new List<string>(){ fromBodyRightsModel.Name }));
                    return BadRequest(messages);
                }
            }
            
            // Rights data validity and logic validity
            messages = sharedValidationHelper.ValidateRights(authUserModel.Application.ApplicationDescriptor,
                                                        fromBodyRightsModel.DataDictionary);
            if (messages.Count != 0)
                return BadRequest(messages);
            
            #endregion

            rightsRepository.SetNameAndData(rightsModel, fromBodyRightsModel.Name, fromBodyRightsModel.DataDictionary);
            messages.Add(new Message(MessageTypeEnum.Info, 
                                              4007, 
                                              new List<string>(){ fromBodyRightsModel.Name }));
            return Ok(messages);
        }
    }
}