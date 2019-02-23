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
using RazorWebApp.Repositories;
using RazorWebApp.Helpers;
using System.Security.Claims;
using SharedLibrary.Enums;
using Server;

namespace RazorWebApp.Controllers.User
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
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            // Authorization
            if (!controllerHelper.Authorize(authUserModel, (long)SystemDatasetsEnum.Users, RightsEnum.CRUD))
                return Forbid();
            // Get data from database
            var userRepository = new UserRepository(_context);
            var userModel = userRepository.GetById(authUserModel.ApplicationId, id);
            if (userModel == null)
                return BadRequest($"ERROR: Combination of application name \"{authUserModel.Application.LoginApplicationName}\" and user id \"{id}\" does not exist.");
            //TODO VALIDACE
            // zajistit ze posledni uzivatel nepujde smazat - vzdy musi byt alespon 1 uzivatel aplikace
            if (userRepository.GetAllByApplicationId(authUserModel.ApplicationId).Count <= 1)
                return BadRequest("ERROR: Cannot delete last user in application. There has to be always at least one.");
            userRepository.Remove(userModel);
            return Ok($"INFO: User \"{userModel.GetUsername()}\" deleted successfully.");
        }
    }
}