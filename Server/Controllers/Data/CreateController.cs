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
            ApplicationModel application = (from a in _context.ApplicationDbSet
                                   where (a.Name == appName)
                                   select a).FirstOrDefault();
            if (application == null)
                return BadRequest($"ERROR: Application name {appName} does not exist.");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            var datasetId = adh.GetDatasetIdByName(datasetName);
            if (datasetId == null)
                return BadRequest($"ERROR: Dataset name {datasetName} does not exist.");

            string JsonData = JsonConvert.SerializeObject(data);
            DataModel dataModel = new DataModel{ApplicationId=application.Id, DatasetId=(long)datasetId, Data=JsonData};
            _context.DataDbSet.Add(dataModel);

            _context.SaveChanges();
            return Ok($"New data in dataset {datasetName} created successfully.");
        }
    }
}