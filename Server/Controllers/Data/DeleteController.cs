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
            var applicationRepository = new ApplicationRepository(_context);
            var application = applicationRepository.GetByLoginApplicationName(appName);
            // ApplicationModel application = (from a in _context.ApplicationDbSet
            //                        where (a.LoginApplicationName == appName)
            //                        select a).FirstOrDefault();
            if (application == null)
                return BadRequest($"ERROR: Application name {appName} does not exist.");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            var datasetId = adh.GetDatasetIdByName(datasetName);
            if (datasetId == null)
                return BadRequest($"ERROR: Dataset name {datasetName} does not exist.");
            var dataRepository = new DataRepository(_context);
            //TODO kontrolovat id+dataset+aplikaci
            var dataModel = dataRepository.GetById(id);
            // DataModel dataModel = (from p in _context.DataDbSet
            //                        where (p.Application.LoginApplicationName == appName && p.DatasetId == datasetId && p.Id == id)
            //                        select p).FirstOrDefault();
            if (dataModel == null)
                return BadRequest($"ERROR: Combination of application name {appName}, dataset {datasetName} and id {id} does not exist.");
            dataRepository.Remove(dataModel);
            // _context.DataDbSet.Remove(query);
            // _context.SaveChanges();
            return Ok($"Data  from dataset {datasetName} deleted successfully.");
        }
    }
}