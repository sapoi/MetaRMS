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
using System.Security.Claims;
using Server.Helpers;
using SharedLibrary.Enums;

namespace Server.Controllers.Data
{
    [Route("api/data/[controller]")]
    public class CreateController : Controller
    {
        private readonly DatabaseContext _context;

        public CreateController(DatabaseContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpPost]
        [Route("{appName}/{datasetName}")]
        public IActionResult Create(string appName, string datasetName, 
                                      [FromBody] Dictionary<string, object> data)
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
            if (!controllerHelper.Authorize(authUserModel, datasetDescriptor.Id, RightsEnum.CRU))
                return Forbid();
            //TODO Input data validations
            var dataRepository = new DataRepository(_context);
            string JsonData = JsonConvert.SerializeObject(data);
            DataModel dataModel = new DataModel{ApplicationId = authUserModel.ApplicationId, 
                                                DatasetId = datasetDescriptor.Id, 
                                                Data = JsonData};
            dataRepository.Add(dataModel);
            return Ok($"INFO: New data in dataset \"{datasetName}\" created successfully.");




            // var identity = HttpContext.User.Identity as ClaimsIdentity;
            // var applicationRepository = new ApplicationRepository(_context);
            // var application = applicationRepository.GetByLoginApplicationName(appName);
            // // ApplicationModel application = (from a in _context.ApplicationDbSet
            // //                        where (a.LoginApplicationName == appName)
            // //                        select a).FirstOrDefault();
            // if (application == null)
            //     return BadRequest($"ERROR: Application name {appName} does not exist.");
            // ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptor);
            // var datasetDescriptor = adh.GetDatasetDescriptorByName(datasetName);
            // if (datasetDescriptor == null)
            //     return BadRequest($"ERROR: Dataset name {datasetName} does not exist.");
            // // var datasetId = adh.GetDatasetIdByName(datasetName);
            // // if (datasetId == null)
            // //     return BadRequest($"ERROR: Dataset name {datasetName} does not exist.");

            // string JsonData = JsonConvert.SerializeObject(data);
            // DataModel dataModel = new DataModel{ApplicationId=application.Id, DatasetId=datasetDescriptor.Id, Data=JsonData};
            // var dataRepository = new DataRepository(_context);
            // dataRepository.Add(dataModel);
            // // _context.DataDbSet.Add(dataModel);

            // // _context.SaveChanges();
            // return Ok($"New data in dataset {datasetName} created successfully.");
        }
    }
}