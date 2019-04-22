using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        /// API endpoint for deleting data.
        /// </summary>
        /// <param name="datasetId">Id of dataset the data are from</param>
        /// <param name="id">Id of the data</param>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If data successfully deleted</response>
        /// <response code="401">If user is not authenticated</response>
        /// <response code="403">If user is not autorized to delete data</response>
        /// <response code="404">If input is not valid or data can not be deleted</response>
        [Authorize]
        [HttpDelete]
        [Route("{datasetId}/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult DeleteById(long datasetId, long id)
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
            if (!AuthorizationHelper.IsAuthorized(authUserModel, datasetDescriptor.Id, RightsEnum.CRUD))
                return Forbid();

            #region VALIDATIONS

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
                Logger.LogMessagesToConsole(messages);
                return BadRequest(messages);
            }
           
            // Delete validity check
            using (var transaction = context.Database.BeginTransaction())
            {
                // Set to delete or remove all references from data referencing dataModel to delete
                if (controllerHelper.IfCanBeDeletedPerformDeleteActions(authUserModel, dataModel))
                {
                    // Remove model itself
                    dataRepository.Remove(dataModel);
                    // Save all performed changes into the database
                    transaction.Commit();
                    messages.Add(new Message(MessageTypeEnum.Info, 
                                              2004, 
                                              new List<string>(){ datasetDescriptor.Name }));
                    return Ok(messages);
                }
                // DataModel to delete or other model that should be deleted can not be deleted
                transaction.Rollback();
                messages.Add(new Message(MessageTypeEnum.Error, 
                                              2012, 
                                              new List<string>()));
                return BadRequest(messages);
            }

            #endregion
        }
    }
}