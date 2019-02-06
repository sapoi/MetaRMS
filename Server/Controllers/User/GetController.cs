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
using Server.Repositories;
using Server.Helpers;

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
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            // Authorization
            if (!controllerHelper.Authorize(authUserModel, (long)SystemDatasetsEnum.Users, RightsEnum.R))
                return Forbid();
            // Get data from database
            var userRepository = new UserRepository(_context);
            List<UserModel> allUsers = userRepository.GetAllByApplicationId(authUserModel.ApplicationId);
            // Prepare data for client
            DataHelper dataHelper = new DataHelper(_context, authUserModel.Application, (long)SystemDatasetsEnum.Users);
            foreach (var row in allUsers)
            {
                // // ignore large JSON data
                // // row.Application = null;
                // row.Rights.Application = null;
                // row.Rights.Users = null;
                // // prepare data for client - add text representation for references
                dataHelper.PrepareOneRowForClient(row);
            }
            return Ok(allUsers);



            // // get logged user's identity from HttpContext
            // var identity = HttpContext.User.Identity as ClaimsIdentity;
            // // if user is authenticated and JWT contains claim named LoginApplicationName
            // if (!identity.IsAuthenticated || identity.FindFirst("LoginApplicationName").Value != appName) //TODO muze spadnout
            //     // user is not authorized to access application appName
            //     return Unauthorized();

            // var applicationRepository = new ApplicationRepository(_context);
            // var application = applicationRepository.GetByLoginApplicationName(appName);
            // // ApplicationModel application = (from a in _context.ApplicationDbSet
            // //                                 where (a.LoginApplicationName == appName)
            // //                                 select a).FirstOrDefault();
            // if (application == null)
            //     return BadRequest("spatny nazev aplikace neexistuje");
            // // ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            // // check if user has rights to see dataset datasetName
            // // get user id from identity
            // var claim = identity.FindFirst("UserId"); //TODO muze spadnout
            // if (claim == null)
            //     return BadRequest("v claimu chybi userid");
            // long userId;
            // if (!long.TryParse(claim.Value, out userId))
            //     return BadRequest("value pro userid neni ve spravnem formatu");

            // var userRepository = new UserRepository(_context);
            // var user = userRepository.GetById(userId);
            // // var user = (from u in _context.UserDbSet
            // //             where u.Id == userId && u.ApplicationId == application.Id
            // //             select u).FirstOrDefault();
            // if (user == null)
            //     return BadRequest("TODO");


            // // read user's rights from DB
            // // var rights = (from r in _context.RightsDbSet
            // //               where r.Id == user.RightsId
            // //               select r).FirstOrDefault();
            // var rights = user.Rights;
            // if (rights == null)
            //     return BadRequest("prava neexistuji");
            // var rightsDict = JsonConvert.DeserializeObject<Dictionary<long, RightsEnum>>(rights.Data);
            // var rightsRights = rightsDict.Where(r => r.Key == (long)SystemDatasetsEnum.Rights).FirstOrDefault();
            // if (rightsRights.Equals(default(KeyValuePair<long, RightsEnum>)))
            //     return BadRequest("v pravech neni rights");
            // if (rightsRights.Value <= RightsEnum.None)
            //     return Forbid();

            // List<UserModel> allUsers = userRepository.GetAllByApplicationId(application.Id);
            // // List<UserModel> allUsers = _context.UserDbSet.Where(u => u.Application.LoginApplicationName == appName)
            // //                                           .Include(u => u.Rights)
            // //                                           .ToList();

            // DataHelper dataHelper = new DataHelper(_context, application, (long)SystemDatasetsEnum.Users);
            // foreach (var row in allUsers)
            // {
            //     // ignore large JSON data
            //     // row.Application = null;
            //     row.Rights.Application = null;
            //     row.Rights.Users = null;
            //     // prepare data for client - add text representation for references
            //     dataHelper.PrepareOneRowForClient(row);
            // }
            // return Ok(allUsers);

            // // return Ok(query);
        }
        [Authorize]
        [HttpGet]
        [Route("{appName}/{id}")]
        public IActionResult GetById(string appName, long id)
        {
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var requestUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (requestUserModel == null)
                return Unauthorized();
            // Authorization
            if (!controllerHelper.Authorize(requestUserModel,(long)SystemDatasetsEnum.Users, RightsEnum.R))
                return Forbid();
            // Get data from database
            var userRepository = new UserRepository(_context);
            var userModel = userRepository.GetById(id);
            if (userModel == null)
                return BadRequest($"No user with id {id} found.");
            // Prepare data for client
            // // ignore large JSON data
            // //userModel.Application = null;
            // userModel.Rights.Application = null;
            // userModel.Rights.Users = null;
            return Ok(userModel);


            // var applicationRepository = new ApplicationRepository(_context);
            // var application = applicationRepository.GetByLoginApplicationName(appName);
            // // ApplicationModel application = (from a in _context.ApplicationDbSet
            // //                                 where (a.LoginApplicationName == appName)
            // //                                 select a).FirstOrDefault();
            // if (application == null)
            //     return BadRequest("spatny nazev aplikace neexistuje");
            // //ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            // var userRepository = new UserRepository(_context);
            // var userModel = userRepository.GetById(id);
            // // var userModel = _context.UserDbSet.Where(u => u.Application.LoginApplicationName == appName && u.Id ==id)
            // //                               .Include(u => u.Rights)
            // //                               .FirstOrDefault();
            // if (requestUserModel == null)
            //     return BadRequest("neexistujici kombinace jmena aplikace, datasetu a id");

            // // ignore large JSON data
            // //userModel.Application = null;
            // requestUserModel.Rights.Application = null;
            // requestUserModel.Rights.Users = null;
            
            // return Ok(requestUserModel);
        }
    }
}