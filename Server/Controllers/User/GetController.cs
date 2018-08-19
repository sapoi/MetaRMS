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
using JsonDotNet.CustomContractResolvers;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers.User
{
    [Route("api/user/[controller]")]
    public class GetController : Controller
    {
        private readonly DatabaseContext _context;

        public GetController(DatabaseContext context)
        {
            _context = context;
        }
        ///
        /// if user from HttpContext has at least Read rights to rights table returns all rights data
        //TODO handle BadRequest, Forbid, Unauthorized
        ///
        [Authorize]
        [HttpGet]
        [Route("{appName}")]
        public IActionResult GetAll(string appName)
        {
            // get logged user's identity from HttpContext
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            // if user is authenticated and JWT contains claim named ApplicationName
            if (!identity.IsAuthenticated || identity.FindFirst("ApplicationName").Value != appName) //TODO muze spadnout
                // user is not authorized to access application appName
                return Unauthorized();

            ApplicationModel application = (from a in _context.ApplicationDbSet
                                            where (a.Name == appName)
                                            select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
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
            var rightsRights = rightsDict.Where(r => r.Key == (long)SystemDatasetsEnum.Rights).FirstOrDefault();
            if (rightsRights.Equals(default(KeyValuePair<long, RightsEnum>)))
                return BadRequest("v pravech neni rights");
            if (rightsRights.Value <= RightsEnum.None)
                return Forbid();

            List<UserModel> query = _context.UserDbSet.Where(u => u.Application.Name == appName)
                                                      .Include(u => u.Rights)
                                                      .ToList();
                                                      
            // ignore large JSON data
            foreach (var row in query)
            {
                row.Application = null;
                row.Rights.Application = null;
                row.Rights.Users = null;
            }
            return Ok(query);
        }
        [Authorize]
        [HttpGet]
        [Route("{appName}/{id}")]
        public IActionResult GetById(string appName, long id)
        {
            ApplicationModel application = (from a in _context.ApplicationDbSet
                                            where (a.Name == appName)
                                            select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            
            var query = _context.UserDbSet.Where(u => u.Application.Name == appName && u.Id ==id)
                                          .Include(u => u.Rights)
                                          .FirstOrDefault();
            if (query == null)
                return BadRequest("neexistujici kombinace jmena aplikace, datasetu a id");

            // ignore large JSON data
            query.Application = null;
            query.Rights.Application = null;
            query.Rights.Users = null;
            
            return Ok(query);
        }
    }
}