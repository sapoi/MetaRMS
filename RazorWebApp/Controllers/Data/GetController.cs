using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using SharedLibrary.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;
using RazorWebApp.Repositories;
using RazorWebApp.Helpers;
using System.Collections.Generic;
using SharedLibrary.Structures;
using Server;

namespace RazorWebApp.Controllers.Data
{
    [Route("api/data/[controller]")]
    public class GetController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;

        public GetController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for getting all data by dataset id.
        /// </summary>
        /// <returns>List of DataModels or messages about action result</returns>
        /// <response code="200">If data successfully sent</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to read data</response>
        /// <response code="404">If datasetId is not valid</response>
        [Authorize]
        [HttpGet]
        [Route("{datasetId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult GetAll(long datasetId)
        {
            // List of messages
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Dataset descriptor
            var datasetDescriptor = authUserModel.Application.ApplicationDescriptor.Datasets.FirstOrDefault(d => d.Id == datasetId);
            if (datasetDescriptor == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  2001, 
                                                  new List<string>(){ datasetId.ToString() }));
                Logger.LogMessagesToConsole(messages);
                return BadRequest(messages);
            }

            // Authorization
            if (!controllerHelper.Authorize(authUserModel, datasetDescriptor.Id, RightsEnum.R))
                return Forbid();

            // Get data from database
            var dataRepository = new DataRepository(context);
            var dataModelList = dataRepository.GetAllByApplicationIdAndDatasetId(authUserModel.ApplicationId, datasetDescriptor.Id);

            // Prepare data for client
            DataHelper dataHelper = new DataHelper(context, authUserModel.Application, datasetDescriptor.Id);
            foreach (var item in dataModelList)
                dataHelper.PrepareOneRowForClient(item);

            return Ok(dataModelList);
        }

        /// <summary>
        /// API endpoint for getting data by dataset id and data id.
        /// </summary>
        /// <returns>DataModel or messages about action result</returns>
        /// <response code="200">If data successfully sent</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to read data</response>
        /// <response code="404">If datasetId and/or id is not valid</response>
        [Authorize]
        [HttpGet]
        [Route("{datasetId}/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult GetById(long datasetId, long id)
        {
            // List of messages
            var messages = new List<Message>();

            // Authentication
            var controllerHelper = new ControllerHelper(context);
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();

            // Dataset descriptor
            var datasetDescriptor = authUserModel.Application.ApplicationDescriptor.Datasets.FirstOrDefault(d => d.Id == datasetId);
            if (datasetDescriptor == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  2001, 
                                                  new List<string>(){ datasetId.ToString() }));
                Logger.LogMessagesToConsole(messages);
                return BadRequest(messages);
            }
            
            // Authorization
            if (!controllerHelper.Authorize(authUserModel, datasetDescriptor.Id, RightsEnum.R))
                return Forbid();

            // Get data from database
            var dataRepository = new DataRepository(context);
            var dataModel = dataRepository.GetById(authUserModel.ApplicationId, datasetDescriptor.Id, id);
            if (dataModel == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  2006, 
                                                  new List<string>(){ id.ToString(), 
                                                                      datasetDescriptor.Name 
                                                                    }));
                return BadRequest(messages);
            }

            // Prepare data for client
            DataHelper dataHelper = new DataHelper(context, authUserModel.Application, datasetDescriptor.Id);
            dataHelper.PrepareOneRowForClient(dataModel);
            
            return Ok(dataModel);
        }
    }
}