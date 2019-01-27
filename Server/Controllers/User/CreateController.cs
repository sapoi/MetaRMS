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
            var applicationRepository = new ApplicationRepository(_context);
            var application = applicationRepository.GetByLoginApplicationName(appName);
            // ApplicationModel application = (from a in _context.ApplicationDbSet
            //                        where (a.LoginApplicationName == appName)
            //                        select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");

            string JsonData = JsonConvert.SerializeObject(fromBodyUserModel.DataDictionary);
            // string hashedPassword = PasswordHelper.ComputeHash(fromBodyUserModel.GetUsername());
            UserModel userModel = new UserModel{ ApplicationId = application.Id, 
                                                 Application = application,
                                                 //Username = fromBodyUserModel.Username, 
                                                //  Password = hashedPassword,
                                                 Data = JsonData,
                                                 RightsId = fromBodyUserModel.RightsId
                                                };
            //INPUT VALIDATIONS
            // check if username is nonempty
            if (userModel.GetUsername() == "")
                return BadRequest("Username can not be an empty string.");

            
            var userRepository = new UserRepository(_context);

            // check if user name is unique
            var sameNameUser = userRepository.GetByApplicationIdAndUsername(application.Id, userModel.GetUsername());
            // var sameNameUser = _context.UserDbSet.Where(u => u.ApplicationId == application.Id && 
            //                                                   u.GetUsername() == dataModel.GetUsername()).ToList();
            if (sameNameUser != null)
                return BadRequest($"User named {userModel.GetUsername()} already exists, please chhose another username.");

            // reset passwword to default
            userRepository.ResetPassword(userModel);

            userRepository.Add(userModel);

            // // reset passwword to default
            // userRepository.ResetPassword(userModel);


            // _context.UserDbSet.Add(userModel);
            // _context.SaveChanges();
            return Ok($"New user {userModel.GetUsername()} created successfully.");
        }
    }
}