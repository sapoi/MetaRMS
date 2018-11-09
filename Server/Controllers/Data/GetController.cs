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
using System.Security.Claims;
using SharedLibrary.Enums;

namespace Server.Controllers.Data
{
    [Route("api/data/[controller]")]
    public class GetController : Controller
    {
        private readonly DatabaseContext _context;

        public GetController(DatabaseContext context)
        {
            _context = context;
        }
        ///
        /// if user from HttpContext has at least Read rights to dataset from parameter returns all dataset data
        //TODO handle BadRequest, Forbid, Unauthorized
        ///
        [Authorize]
        [HttpGet]
        [Route("{appName}/{datasetName}")]
        public IActionResult GetAll(string appName, string datasetName)
        {
            // get logged user's identity from HttpContext
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            // if user is authenticated and JWT contains claim named ApplicationName
            if (!identity.IsAuthenticated || identity.FindFirst("ApplicationName").Value != appName) //TODO muze spadnout
                // user is not authorized to access application appName
                return Unauthorized();

            ApplicationModel application = (from a in _context.ApplicationDbSet
                                            where (a.LoginApplicationName == appName)
                                            select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            var datasetId = adh.GetDatasetIdByName(datasetName);
            if (datasetId == null)
                return BadRequest("spatny nazev datasetu");
            // check if user has rights to see dataset datasetName
            // get user id from identity
            var claim = identity.FindFirst("UserId"); //TODO muze spadnout
            if (claim == null)
                return BadRequest("v claimu chybi userid");
            long userId;
            if (!long.TryParse(claim.Value, out userId))
                return BadRequest("value pro userid neni ve spravnem formatu");
            var user = (from u in _context.UserDbSet
                        where u.Id == userId && u.ApplicationId == application.Id
                        select u).FirstOrDefault();
            if (user == null)
                return BadRequest("TODO");
            // read user's rights from DB
            var rights = (from r in _context.RightsDbSet
                          where r.Id == user.RightsId
                          select r).FirstOrDefault();
            if (rights == null)
                return BadRequest("prava neexistuji");
            var rightsDict = JsonConvert.DeserializeObject<Dictionary<long, RightsEnum>>(rights.Data);
            var datasetRights = rightsDict.Where(r => r.Key == (long)datasetId).FirstOrDefault();
            if (datasetRights.Equals(default(KeyValuePair<long, RightsEnum>)))
                return BadRequest("v pravech neni dataset s datasetId");
            if (datasetRights.Value <= RightsEnum.None)
                return Forbid();
            List<DataModel> query = (from p in _context.DataDbSet
                                     where (p.Application.LoginApplicationName == appName && p.DatasetId == datasetId)
                                     select p).ToList();
            List<Dictionary<String, Object>> result = new List<Dictionary<String, Object>>();
            foreach (var d in query)
            {
                var tmpDict = d.DataDictionary;
                // serializing list containing DBId, because every data is expected to be in a list
                tmpDict.Add("DBId", new List<object>() { d.Id } );
                result.Add(tmpDict);
            }
            return Ok(result);
        }
        [Authorize]
        [HttpGet]
        [Route("{appName}/{datasetName}/{id}")]
        public IActionResult GetById(string appName, string datasetName, long id)
        {
            ApplicationModel application = (from a in _context.ApplicationDbSet
                                            where (a.LoginApplicationName == appName)
                                            select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            var datasetId = adh.GetDatasetIdByName(datasetName);
            if (datasetId == null)
                return BadRequest("spatny nazev datasetu");
            DataModel query = (from p in _context.DataDbSet
                               where (p.Application.LoginApplicationName == appName && p.DatasetId == datasetId && p.Id == id)
                               select p).FirstOrDefault();
            if (query == null)
                return BadRequest("neexistujici kombinace jmena aplikace, datasetu a id");




            return Ok(query.DataDictionary);
        }
    }
}