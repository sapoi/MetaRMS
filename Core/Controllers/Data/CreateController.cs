using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using System.Linq;
using SharedLibrary.Helpers;
using System.Security.Claims;
using Core.Helpers;
using Core.Repositories;
using SharedLibrary.Enums;
using SharedLibrary.Structures;

namespace Core.Controllers.Data
{
    [Route("api/data/[controller]")]
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
        /// API endpoint for creating data.
        /// </summary>
        /// <param name="fromBodyDataModel">New DataModel</param>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If data successfully created</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to create data</response>
        /// <response code="404">If input is not valid</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult Create([FromBody] DataModel fromBodyDataModel)
        {
            // List of messages to return to the client
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            
            // Dataset descriptor
            var datasetDescriptor = authUserModel.Application.ApplicationDescriptor.Datasets.FirstOrDefault(d => d.Id == fromBodyDataModel.DatasetId);
            if (datasetDescriptor == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  2001, 
                                                  new List<string>(){ fromBodyDataModel.DatasetId.ToString() }));
                Logger.LogMessagesToConsole(messages);
                return BadRequest(messages);
            }

            // Authorization
            if (!AuthorizationHelper.IsAuthorized(authUserModel, datasetDescriptor.Id, RightsEnum.CRU))
                return Forbid();

            #region VALIDATIONS

            // New data ApplicationId must be the same as of authorized user
            var sharedValidationHelper = new SharedValidationHelper();
            messages = sharedValidationHelper.ValidateApplicationId(fromBodyDataModel.ApplicationId, authUserModel.ApplicationId);
            if (messages.Count != 0)
                return BadRequest(messages);
            fromBodyDataModel.Application = authUserModel.Application;

            // Input data validations
            var validReferencesIdsDictionary = controllerHelper.GetAllReferencesIdsDictionary(authUserModel.Application);
            messages = sharedValidationHelper.ValidateDataByApplicationDescriptor(datasetDescriptor, 
                                                                             fromBodyDataModel.DataDictionary, 
                                                                             validReferencesIdsDictionary);
            if (messages.Count != 0)
                return BadRequest(messages);

            #endregion

            var dataRepository = new DataRepository(context);
            dataRepository.Add(fromBodyDataModel);
            messages.Add(new Message(MessageTypeEnum.Info, 
                                              2002, 
                                              new List<string>(){ datasetDescriptor.Name }));
            return Ok(messages);
        }
    }
}