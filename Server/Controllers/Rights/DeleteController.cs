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
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            // Authorization
            if (!controllerHelper.Authorize(authUserModel, (long)SystemDatasetsEnum.Rights, RightsEnum.CRUD))
                return Forbid();
            // Get data from database
            var rightsRepository = new RightsRepository(_context);
            var rightsModel = rightsRepository.GetById(authUserModel.ApplicationId, id);
            if (rightsModel == null)
                return BadRequest($"ERROR: Combination of application name \"{authUserModel.Application.LoginApplicationName}\" and rights id \"{id}\" does not exist.");
            //TODO validations
            // check if no users are using rights to delete
            var userRepository = new UserRepository(_context);
            List<UserModel> users = userRepository.GetByRightsId(rightsModel.Id);
            if (users.Count > 0)
            {
                string usernames = users[0].GetUsername();
                for (int i = 1; i < users.Count; i++)
                    usernames += ", " + users[i].GetUsername();
                return BadRequest($"ERROR: Can't delete - rights {rightsModel.Name} are used by one or more users: {usernames}.");
            }
            rightsRepository.Remove(rightsModel);
            return Ok($"INFO: Rights \"{rightsModel.Name}\" deleted successfully.");
        }
    }
}