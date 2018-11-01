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
    public class CreateController : Controller
    {
        private readonly DatabaseContext _context;

        public CreateController(DatabaseContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpPost]
        [Route("{appName}")]
        public IActionResult Create(string appName, [FromBody] UserModel fromBodyUserModel)
        {
            ApplicationModel application = (from a in _context.ApplicationDbSet
                                   where (a.LoginApplicationName == appName)
                                   select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");

            string JsonData = JsonConvert.SerializeObject(fromBodyUserModel.DataDictionary);
            string hashedPassword = PasswordHelper.ComputeHash(fromBodyUserModel.Password);
            UserModel dataModel = new UserModel{ ApplicationId=application.Id, 
                                                 Username = fromBodyUserModel.Username, 
                                                 Password = hashedPassword,
                                                 Data = JsonData,
                                                 RightsId = fromBodyUserModel.RightsId
                                                };

            // check if user name is unique
            var sameNameUsers = _context.UserDbSet.Where(u => u.ApplicationId == application.Id && 
                                                              u.Username == dataModel.Username).ToList();
            if (sameNameUsers.Count > 0)
                return BadRequest($"User named {dataModel.Username} already exists, please chhose another username.");

            _context.UserDbSet.Add(dataModel);
            _context.SaveChanges();
            return Ok($"New user {dataModel.Username} created successfully.");
        }
    }
}