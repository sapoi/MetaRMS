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

namespace Server.Controllers.User
{
    [Route("api/user/[controller]")]
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
                                   where (a.Name == appName)
                                   select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);

            UserModel user = (from p in _context.UserDbSet
                               where (p.ApplicationId == application.Id && p.Id == id)
                               select p).FirstOrDefault();
            if (user == null)
                return BadRequest("neexistujici kombinace jmena aplikace a id");
            
            // zajistit ze posledni uzivatel nepujde smazat - vzdy musi byt alespon 1 uzivatel aplikace
            var allAppUsers = _context.UserDbSet.Where(u => u.ApplicationId == application.Id).ToList();
            if (allAppUsers.Count <= 1)
                return BadRequest("ERROR: Cannot delete last user in application. There has to be always at least one.");
        
            _context.UserDbSet.Remove(user);
            _context.SaveChanges();
            return Ok($"User {user.Username} deleted successfully.");
        }
    }
}