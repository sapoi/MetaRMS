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

namespace Server.Controllers.Rights
{
    [Route("api/rights/[controller]")]
    public class PatchController : Controller
    {
        private readonly DatabaseContext _context;

        public PatchController(DatabaseContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpPost]
        [Route("{appName}/{id}")]
        public IActionResult PatchById(string appName, long id,  [FromBody] RightsModel fromBodyRightsModel)
        {
            ApplicationModel application = (from a in _context.ApplicationDbSet
                                   where (a.Name == appName)
                                   select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            RightsModel query = (from p in _context.RightsDbSet
                                   where (p.Application.Name == appName && p.Id == id)
                                   select p).FirstOrDefault();
            if (query == null)
                return BadRequest("neexistujici kombinace jmena aplikace a id");

            // string JsonData = JsonConvert.SerializeObject(rightsData);
            query.Name = fromBodyRightsModel.Name;
            query.Data = JsonConvert.SerializeObject(fromBodyRightsModel.DataDictionary);
            _context.SaveChanges();
            return Ok($"Rights {fromBodyRightsModel.Name} editted successfully.");
        }
    }
}