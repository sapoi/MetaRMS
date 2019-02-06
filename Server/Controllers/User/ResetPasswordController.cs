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
    public class ResetPasswordController : Controller
    {
        private readonly DatabaseContext _context;

        public ResetPasswordController(DatabaseContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet]
        [Route("{appName}/{id}")]
        public IActionResult ResetPasswordById(string appName, long id)
        {
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            // Authorization
            if (!controllerHelper.Authorize(authUserModel, (long)SystemDatasetsEnum.Rights, RightsEnum.RU))
                return Forbid();
            // Get data from database
            var userRepository = new UserRepository(_context);
            var userModel = userRepository.GetById(authUserModel.ApplicationId, id);
            if (userModel == null)
                return BadRequest($"ERROR: Combination of application name \"{authUserModel.Application.LoginApplicationName}\" and user id \"{id}\" does not exist.");
            userRepository.ResetPassword(userModel);
            return Ok($"INFO: User \"{userModel.GetUsername()}\" password resetted successfully to \"{userModel.GetUsername()}\".");




            // var applicationRepository = new ApplicationRepository(_context);
            // var application = applicationRepository.GetByLoginApplicationName(appName);
            // // ApplicationModel application = (from a in _context.ApplicationDbSet
            // //                        where (a.LoginApplicationName == appName)
            // //                        select a).FirstOrDefault();
            // if (application == null)
            //     return BadRequest("spatny nazev aplikace neexistuje");
            // //ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);

            // var userRepository = new UserRepository(_context);
            // var userModel = userRepository.GetById(id);
            // // UserModel userModel = (from p in _context.UserDbSet
            // //                    where (p.ApplicationId == application.Id && p.Id == id)
            // //                    select p).FirstOrDefault();
            // if (userModel == null)
            //     return BadRequest("neexistujici kombinace jmena aplikace a id");
            
            // // set user's password to his username and save it hashed to database
            // userRepository.ResetPassword(userModel);
            // // string username = userModel.GetUsername();
            // // userModel.Password = PasswordHelper.ComputeHash(username);
            // // _context.SaveChanges();
            // return Ok($"User {userModel.GetUsername()} password resetted successfully to {userModel.GetUsername()}.");
        }
    }
}