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
using Server.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;

namespace Server.Controllers.User
{
    [Route("api/user/[controller]")]
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
        public IActionResult PatchById(string appName, long id,  [FromBody] UserModel fromBodyUserModel)
        {
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            // Authorization
            if (!controllerHelper.Authorize(authUserModel, (long)SystemDatasetsEnum.Users, RightsEnum.RU))
                return Forbid();
            // Get data from database
            var userRepository = new UserRepository(_context);
            var userModel = userRepository.GetById(authUserModel.ApplicationId, id);
            if (userModel == null)
                return BadRequest($"ERROR: Combination of application name \"{authUserModel.Application.LoginApplicationName}\" and user id \"{id}\" does not exist.");
            // input prepare - set application - the same as user from database with the same id
            fromBodyUserModel.ApplicationId = userModel.ApplicationId;
            fromBodyUserModel.Application = userModel.Application;
            //TODO Input data validations
            //INPUT VALIDATIONS
            // check if username is nonempty
            if (fromBodyUserModel.GetUsername() == "")
                return BadRequest("ERROR: Username can not be an empty string.");
            // username unique
            if (userModel.GetUsername() != fromBodyUserModel.GetUsername()) {
                var sameNameUser = userRepository.GetByApplicationIdAndUsername(authUserModel.ApplicationId, fromBodyUserModel.GetUsername());
                if (sameNameUser != null)
                    return BadRequest($"ERROR: User named \"{fromBodyUserModel.GetUsername()}\" already exists, please choose another username.");
            }

            userRepository.SetRightsIdAndData(userModel, fromBodyUserModel.RightsId, fromBodyUserModel.DataDictionary);
            return Ok($"INFO: User \"{fromBodyUserModel.GetUsername()}\" editted successfully.");



            // var applicationRepository = new ApplicationRepository(_context);
            // var application = applicationRepository.GetByLoginApplicationName(appName);
            // // ApplicationModel application = (from a in _context.ApplicationDbSet
            // //                        where (a.LoginApplicationName == appName)
            // //                        select a).FirstOrDefault();
            // if (application == null)
            //     return BadRequest("spatny nazev aplikace neexistuje");
            // // ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            // var userRepository = new UserRepository(_context);
            // var userModel = userRepository.GetById(id);
            // // UserModel userModel = (from p in _context.UserDbSet
            // //                    where (p.Application.LoginApplicationName == appName && p.Id == id)
            // //                    select p).FirstOrDefault();
            // if (userModel == null)
            //     return BadRequest("neexistujici kombinace jmena aplikace a id");

            // // if username has changed, check if the new one is unique
            // string username = userModel.GetUsername();
            // // fromBodyUserModel needs to have .Application attribute in order to call .GetUsername()
            // // .Application must be the same as userModel.Application
            // fromBodyUserModel.Application = userModel.Application;
            // //INPUT VALIDATIONS
            // // check if username is nonempty
            // if (fromBodyUserModel.GetUsername() == "")
            //     return BadRequest("Username can not be an empty string.");
            
            // if (username != fromBodyUserModel.GetUsername()) {
            //     var sameNameUser = userRepository.GetByApplicationIdAndUsername(application.Id, fromBodyUserModel.GetUsername());
            //     // var sameNameUsers = _context.UserDbSet.Where(u => u.ApplicationId == application.Id && 
            //     //                                                   u.Id != id).ToList();
            //     if (sameNameUser != null)
            //         return BadRequest($"User named {fromBodyUserModel.GetUsername()} already exists, please choose another username.");
            // }

            // //query.Username = fromBodyUserModel.Username; now in Data
            // userRepository.SetRightsIdAndData(userModel, fromBodyUserModel.RightsId, fromBodyUserModel.DataDictionary);
            // // userModel.Data = JsonConvert.SerializeObject(fromBodyUserModel.DataDictionary);
            // // userModel.RightsId = fromBodyUserModel.RightsId;
            // // _context.SaveChanges();
            // return Ok($"User {fromBodyUserModel.GetUsername()} editted successfully.");
        }
    }
}