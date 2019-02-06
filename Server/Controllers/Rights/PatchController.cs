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
        public IActionResult PatchById(string appName, long id,  [FromBody] RightsModel fromBodyRightsModel)
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
            var rightsRepository = new RightsRepository(_context);
            var rightsModel = rightsRepository.GetById(authUserModel.ApplicationId, id);
            if (rightsModel == null)
                return BadRequest($"ERROR: Combination of application name \"{authUserModel.Application.LoginApplicationName}\" and rights id \"{id}\" does not exist.");
            // input prepare - set application - the same as user from database with the same id
            fromBodyRightsModel.ApplicationId = authUserModel.ApplicationId;
            fromBodyRightsModel.Application = authUserModel.Application;
            //TODO Input data validations 
            // name unique
            if (rightsModel.Name != fromBodyRightsModel.Name) {
                var sameNameRights = rightsRepository.GetByApplicationIdAndName(authUserModel.ApplicationId, fromBodyRightsModel.Name);
                if (sameNameRights.Count != 0)
                    return BadRequest($"ERROR: Rights named \"{fromBodyRightsModel.Name}\" already exists, please choose another name.");
            }
            rightsRepository.SetNameAndData(rightsModel, fromBodyRightsModel.Name, fromBodyRightsModel.DataDictionary); // TODO nevim kam to dat a jak to pojmenovat
            return Ok($"INFO: Rights \"{fromBodyRightsModel.Name}\" editted successfully.");





            // var applicationRepository = new ApplicationRepository(_context);
            // var application = applicationRepository.GetByLoginApplicationName(appName);
            // // ApplicationModel application = (from a in _context.ApplicationDbSet
            // //                        where (a.LoginApplicationName == appName)
            // //                        select a).FirstOrDefault();
            // if (application == null)
            //     return BadRequest("spatny nazev aplikace neexistuje");
            // //ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            // var rightsRepository = new RightsRepository(_context);
            // //TODO kontrolovat aplikaci
            // var rightsModel = rightsRepository.GetById(id);
            // // RightsModel rightsModel = (from p in _context.RightsDbSet
            // //                        where (p.Application.LoginApplicationName == appName && p.Id == id)
            // //                        select p).FirstOrDefault();
            // if (rightsModel == null)
            //     return BadRequest("neexistujici kombinace jmena aplikace a id");

            // // string JsonData = JsonConvert.SerializeObject(rightsData);
            // // rightsModel.Name = fromBodyRightsModel.Name;
            // // rightsModel.Data = JsonConvert.SerializeObject(fromBodyRightsModel.DataDictionary);
            
            // rightsRepository.SetNameAndData(rightsModel, fromBodyRightsModel.Name, fromBodyRightsModel.DataDictionary); // TODO nevim kam to dat a jak to pojmenovat

            // // _context.SaveChanges();
            // return Ok($"Rights {fromBodyRightsModel.Name} editted successfully.");
        }
    }
}