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
            var applicationRepository = new ApplicationRepository(_context);
            var application = applicationRepository.GetByLoginApplicationName(appName);
            // ApplicationModel application = (from a in _context.ApplicationDbSet
            //                        where (a.LoginApplicationName == appName)
            //                        select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            var rightsRepository = new RightsRepository(_context);
            //TODO kontrolovat aplikaci
            var rightsModel = rightsRepository.GetById(id);
            // RightsModel rightsModel = (from p in _context.RightsDbSet
            //                        where (p.Application.LoginApplicationName == appName && p.Id == id)
            //                        select p).FirstOrDefault();
            if (rightsModel == null)
                return BadRequest("neexistujici kombinace jmena aplikace a id");

            // string JsonData = JsonConvert.SerializeObject(rightsData);
            // rightsModel.Name = fromBodyRightsModel.Name;
            // rightsModel.Data = JsonConvert.SerializeObject(fromBodyRightsModel.DataDictionary);
            
            rightsRepository.SetNameAndData(rightsModel, fromBodyRightsModel.Name, fromBodyRightsModel.DataDictionary); // TODO nevim kam to dat a jak to pojmenovat

            // _context.SaveChanges();
            return Ok($"Rights {fromBodyRightsModel.Name} editted successfully.");
        }
    }
}