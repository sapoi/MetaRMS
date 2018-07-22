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

namespace Server.Controllers
{
    [Route("api/[controller]")]
    public class GetController : Controller
    {
        private readonly DatabaseContext _context;

        public GetController(DatabaseContext context)
        {
            _context = context;
        }
        //[Authorize]
        [HttpGet]
        [Route("{appName}/{datasetName}")]
        public IActionResult GetAll(string appName, string datasetName)
        {
            ApplicationModel application = (from a in _context.ApplicationsDbSet
                                   where (a.Name == appName)
                                   select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            var datasetId = adh.GetDatasetIdByName(datasetName);
            if (datasetId == null)
                return BadRequest("spatny nazev datasetu");

            List<DataModel> query = (from p in _context.DataDbSet
                                   where (p.Application.Name == appName && p.DatasetId == datasetId)
                                   select p).ToList();
            List<Dictionary<String,Object>> result = new List<Dictionary<String,Object>>();
            foreach (var d in query)
            {
                var tmpDict = d.DataDictionary;
                tmpDict.Add("DBId", d.Id);
                result.Add(tmpDict);
            }
            return Ok(result);
        }
    }
}