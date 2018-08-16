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
    public class DeleteController : Controller
    {
        private readonly DatabaseContext _context;

        public DeleteController(DatabaseContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet]
        [Route("{appName}/{id}")]
        public IActionResult DeleteById(string appName, long id)
        {
            ApplicationModel application = (from a in _context.ApplicationsDbSet
                                   where (a.Name == appName)
                                   select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            RightsModel query = (from p in _context.RightsDbSet
                               where (p.ApplicationId == application.Id && p.Id == id)
                               select p).FirstOrDefault();
            if (query == null)
                return BadRequest("neexistujici kombinace jmena aplikace a id");
            _context.RightsDbSet.Remove(query);
            _context.SaveChanges();
            return Ok("deleted successfully");
        }
    }
}