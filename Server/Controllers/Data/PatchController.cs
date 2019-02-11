using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using System.Linq;
using SharedLibrary.Helpers;
using Server.Repositories;
using Server.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;
using SharedLibrary.Structures;

namespace Server.Controllers.Data
{
    [Route("api/data/[controller]")]
    public class PatchController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;

        public PatchController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for patching data.
        /// </summary>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If data successfully patched</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to patch data</response>
        /// <response code="404">If input is not valid</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [Authorize]
        [HttpPost]
        public IActionResult Patch([FromBody] DataModel fromBodyDataModel)
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
            if (!controllerHelper.Authorize(authUserModel, datasetDescriptor.Id, RightsEnum.RU))
                return Forbid();

            #region VALIDATIONS

            // Recieved data ApplicationId must be the same as of authorized user
            var validationsHelper = new ValidationsHelper();
            messages = validationsHelper.ValidateApplicationId(fromBodyDataModel.ApplicationId, authUserModel.ApplicationId);
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
            messages = validationsHelper.ValidateDataByApplicationDescriptor(datasetDescriptor, 
                                                                             dataModel.DataDictionary, 
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