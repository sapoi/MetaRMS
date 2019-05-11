using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using SharedLibrary.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;
using System.Collections.Generic;
using Core.Helpers;
using Core.Repositories;
using SharedLibrary.Structures;

namespace Core.Controllers.Data
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
        /// <param name="datasetId">Id of dataset to get the data from</param>
        /// <returns>List of DataModels or messages about action result</returns>
        /// <response code="200">If data successfully sent</response>
        /// <response code="400">If datasetId is not valid</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to read data</response>
        [Authorize]
        [HttpGet]
        [Route("{datasetId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
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
            if (!AuthorizationHelper.IsAuthorized(authUserModel, datasetDescriptor.Id, RightsEnum.R))
                return Forbid();

            // Get data from database
            var dataRepository = new DataRepository(context);
            var allDataModels = dataRepository.GetAllByApplicationIdAndDatasetId(authUserModel.ApplicationId, datasetDescriptor.Id);

            // Prepare data for client
            DataHelper dataHelper = new DataHelper(context, authUserModel.Application, datasetDescriptor.Id);
            foreach (var item in allDataModels)
            {
                dataHelper.PrepareOneRecordForClient(item);
                // Remove unnecessary data
                item.Application = null;
            }

            return Ok(allDataModels);
        }

        /// <summary>
        /// API endpoint for getting data by dataset id and data id.
        /// </summary>
        /// <param name="datasetId">Id of dataset the data are from</param>
        /// <param name="id">Id of the data to get</param>
        /// <returns>DataModel or messages about action result</returns>
        /// <response code="200">If data successfully sent</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to read data</response>
        /// <response code="400">If datasetId and/or id is not valid</response>
        [Authorize]
        [HttpGet]
        [Route("{datasetId}/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(400)]
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
            if (!AuthorizationHelper.IsAuthorized(authUserModel, datasetDescriptor.Id, RightsEnum.R))
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
            dataHelper.PrepareOneRecordForClient(dataModel);

            // Remove unnecessary data
            dataModel.Application = null;
            
            return Ok(dataModel);
        }
    }
}