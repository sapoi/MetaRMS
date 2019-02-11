using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SharedLibrary.Models;
using System.Linq;
using SharedLibrary.Helpers;
using Server.Repositories;
using Server.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;

namespace Server.Controllers.Data
{
    [Route("api/data/[controller]")]
    public class DeleteController : Controller
    {
        private readonly DatabaseContext _context;

        public DeleteController(DatabaseContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet]
        [Route("{appName}/{datasetName}/{id}")]
        public IActionResult DeleteById(string appName, string datasetName, long id)
        {
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            // Dataset descriptor
            var datasetDescriptor = authUserModel.Application.ApplicationDescriptor.Datasets.FirstOrDefault(d => d.Name == datasetName);
            if (datasetDescriptor == null)
                return BadRequest($"ERROR: Dataset name \"{datasetName}\" not found.");
            // Authorization
            if (!controllerHelper.Authorize(authUserModel, datasetDescriptor.Id, RightsEnum.CRUD))
                return Forbid();
            // Get data from database
            var dataRepository = new DataRepository(_context);
            var dataModel = dataRepository.GetById(authUserModel.ApplicationId, datasetDescriptor.Id, id);
            if (dataModel == null)
                return BadRequest($"ERROR: Combination of application name \"{authUserModel.Application.LoginApplicationName}\", dataset \"{datasetName}\" and id \"{id}\" does not exist.");
            dataRepository.Remove(dataModel);
            return Ok($"INFO: Data from dataset \"{datasetName}\" deleted successfully.");
        }
    }
}