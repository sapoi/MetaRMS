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
            ApplicationModel application = (from a in _context.ApplicationDbSet
                                   where (a.LoginApplicationName == appName)
                                   select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            RightsModel rights = (from p in _context.RightsDbSet
                               where (p.ApplicationId == application.Id && p.Id == id)
                               select p).FirstOrDefault();
            if (rights == null)
                return BadRequest("neexistujici kombinace jmena aplikace a id");
            // check if no users are using rights to delete
            List<UserModel> users = (from u in _context.UserDbSet
                                     where (u.RightsId == rights.Id)
                                     select u).ToList();
            if (users.Count > 0)
            {
                string usernames = users[0].Username;
                for (int i = 1; i < users.Count; i++)
                    usernames += ", " + users[i].Username;
                return BadRequest($"Can't delete - rights {rights.Name} are used by one or more users: {usernames}.");
            }
            _context.RightsDbSet.Remove(rights);
            _context.SaveChanges();
            return Ok($"Rights {rights.Name} deleted successfully.");
        }
    }
}