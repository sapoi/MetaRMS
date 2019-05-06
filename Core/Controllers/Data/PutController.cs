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
        /// API endpoint for putting data.
        /// </summary>
        /// <param name="fromBodyDataModel">Changed DataModel</param>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If data successfully putted</response>
        /// <response code="400">If input is not valid</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to put data</response>
        [Authorize]
        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public IActionResult Put([FromBody] DataModel fromBodyDataModel)
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

            // Recieved data ApplicationId must be the same as of authorized user
            var sharedValidationHelper = new SharedValidationHelper();
            messages = sharedValidationHelper.ValidateApplicationId(fromBodyDataModel.ApplicationId, authUserModel.ApplicationId);
            if (messages.Count != 0)
                return BadRequest(messages);
            fromBodyDataModel.Application = authUserModel.Application;

            // Data must already exist in the database
            var dataRepository = new DataRepository(context);
            var dataModel = dataRepository.GetById(fromBodyDataModel.ApplicationId, fromBodyDataModel.DatasetId, fromBodyDataModel.Id);
            if (dataModel == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  2003, 
                                                  new List<string>(){ fromBodyDataModel.Application.LoginApplicationName,
                                                                      datasetDescriptor.Name,
                                                                      fromBodyDataModel.Id.ToString()
                                                                    }));
                Logger.LogMessagesToConsole(messages);
                return BadRequest(messages);
            }
            
            // Input data validations
            var validReferencesIdsDictionary = controllerHelper.GetAllReferencesIdsDictionary(authUserModel.Application);
            messages = sharedValidationHelper.ValidateDataByApplicationDescriptor(datasetDescriptor, 
                                                                             fromBodyDataModel.DataDictionary, 
                                                                             validReferencesIdsDictionary);
            if (messages.Count != 0)
                return BadRequest(messages);

            #endregion

            dataRepository.SetData(dataModel, fromBodyDataModel.DataDictionary);
            messages.Add(new Message(MessageTypeEnum.Info, 
                                              2005, 
                                              new List<string>(){ datasetDescriptor.Name }));
            return Ok(messages);
        }
    }
}