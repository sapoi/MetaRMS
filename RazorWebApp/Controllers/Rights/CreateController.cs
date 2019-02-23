using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Helpers;
using RazorWebApp.Repositories;
using RazorWebApp.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;
using SharedLibrary.Structures;
using Server;

namespace RazorWebApp.Controllers.Rights
{
    [Route("api/rights/[controller]")]
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
        /// API endpoint for creating rights.
        /// </summary>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If rights successfully created</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to create rights</response>
        /// <response code="404">If input is not valid</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult Create([FromBody] RightsModel fromBodyRightsModel)
        {
            // List of messages to return to the client
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Authorization
            if (!controllerHelper.Authorize(authUserModel, (long)SystemDatasetsEnum.Rights, RightsEnum.CRU))
                return Forbid();

            #region VALIDATIONS

            // New rights ApplicationId must be the same as of authorized user
            var validationsHelper = new ValidationsHelper();
            messages = validationsHelper.ValidateApplicationId(fromBodyRightsModel.ApplicationId, authUserModel.ApplicationId);
            if (messages.Count != 0)
                return BadRequest(messages);
            fromBodyRightsModel.Application = authUserModel.Application;

            // New rights must have a unique name
            var rightsRepository = new RightsRepository(context);
            List<RightsModel> sameNameRights = rightsRepository.GetByApplicationIdAndName(authUserModel.ApplicationId, fromBodyRightsModel.Name);
            if (sameNameRights.Count > 0)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  4001, 
                                                  new List<string>(){ fromBodyRightsModel.Name }));
                return BadRequest(messages);
            }
            
            // Rights data validity and logic validity
            messages = validationsHelper.ValidateRights(authUserModel.Application.ApplicationDescriptor,
                                                        fromBodyRightsModel.DataDictionary);
            if (messages.Count != 0)
                return BadRequest(messages);
            
            #endregion

            rightsRepository.Add(fromBodyRightsModel);
            messages.Add(new Message(MessageTypeEnum.Info, 
                                              4002, 
                                              new List<string>(){ fromBodyRightsModel.Name }));
            return Ok(messages);
        }
    }
}