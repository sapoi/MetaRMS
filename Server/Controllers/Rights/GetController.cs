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
using Server.Repositories;
using Server.Helpers;

namespace Server.Controllers.Rights
{
    [Route("api/rights/[controller]")]
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
            var userModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (userModel == null)
                return Unauthorized();
            // Authorization
            if (!controllerHelper.Authorize(userModel, (long)SystemDatasetsEnum.Rights, RightsEnum.R))
                return Forbid();
            // Get data from database
            var rightsRepository = new RightsRepository(_context);
            List<RightsModel> allRightsForApplication = rightsRepository.GetAllByApplicationId(userModel.ApplicationId);
            // // ignore large JSON data
            // foreach (var row in allRightsForApplication)
            // {
            //     row.Application = null;
            //     row.Users = null;
            // }
            return Ok(allRightsForApplication);





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
            // //ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            // // check if user has rights to see dataset datasetName
            // // get user id from identity
            // var claim = identity.FindFirst("UserId"); //TODO muze spadnout
            // if (claim == null)
            //     return BadRequest("v claimu chybi userid");
            // long userId;
            // if (!long.TryParse(claim.Value, out userId))
            //     return BadRequest("value pro userid neni ve spravnem formatu");
            // var userRepository = new UserRepository(_context);
            // //TODO kontrolovat i aplikaci
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
            // var rightsRepository = new RightsRepository(_context);
            // List<RightsModel> allRightsForApplication = rightsRepository.GetAllByApplicationId(application.Id);
            // // List<RightsModel> query = (from p in _context.RightsDbSet
            // //                          where (p.Application.LoginApplicationName == appName)
            // //                          select p).ToList();
            // // ignore large JSON data
            // foreach (var row in allRightsForApplication)
            // {
            //     row.Application = null;
            //     row.Users = null;
            // }
            // return Ok(allRightsForApplication);
        }
        [Authorize]
        [HttpGet]
        [Route("{appName}/{id}")]
        public IActionResult GetById(string appName, long id)
        {
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            // Authorization
            if (!controllerHelper.Authorize(authUserModel, (long)SystemDatasetsEnum.Rights, RightsEnum.R))
                return Forbid();
            // Get data from database
            var rightsRepository = new RightsRepository(_context);
            var rightsModel = rightsRepository.GetById(id);
            if (rightsModel == null)
                return BadRequest($"ERROR: No rights with id {id} found.");
            // Prepare data for client
            // ignore large JSON data
            rightsModel.Application = null;
            rightsModel.Users = null;
            return Ok(rightsModel);



            // var applicationRepository = new ApplicationRepository(_context);
            // var application = applicationRepository.GetByLoginApplicationName(appName);
            // // ApplicationModel application = (from a in _context.ApplicationDbSet
            // //                                 where (a.LoginApplicationName == appName)
            // //                                 select a).FirstOrDefault();
            // if (application == null)
            //     return BadRequest("spatny nazev aplikace neexistuje");
            // //ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            
            // var rightsRepository = new RightsRepository(_context);
            // //TODO kontrolovat i aplikaci
            // var rightsModel = rightsRepository.GetById(id);
            // // RightsModel rightsModel = (from p in _context.RightsDbSet
            // //                    where (p.Application.LoginApplicationName == appName && p.Id == id)
            // //                    select p).FirstOrDefault();
            // if (rightsModel == null)
            //     return BadRequest("neexistujici kombinace jmena aplikace, datasetu a id");

            // // ignore large JSON data
            // rightsModel.Application = null;
            // rightsModel.Users = null;
            
            // return Ok(rightsModel);
        }
    }
}