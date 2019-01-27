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
        public IActionResult Create(string appName, [FromBody] RightsModel fromBodyRightsModel)
        {
            var applicationRepository = new ApplicationRepository(_context);
            var application = applicationRepository.GetByLoginApplicationName(appName);
            // ApplicationModel application = (from a in _context.ApplicationDbSet
            //                        where (a.LoginApplicationName == appName)
            //                        select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            string JsonData = JsonConvert.SerializeObject(fromBodyRightsModel.DataDictionary);
            
            RightsModel dataModel = new RightsModel{ApplicationId=application.Id, Name=fromBodyRightsModel.Name, Data=JsonData};
            // check if rights name is unique
            var rightsRepository = new RightsRepository(_context);
            List<RightsModel> sameNameRights = rightsRepository.GetByApplicationIdAndName(application.Id, dataModel.Name);
            // var sameNameRights = _context.RightsDbSet.Where(r => r.ApplicationId == application.Id && 
            //                                                      r.Name == dataModel.Name).ToList();
            if (sameNameRights.Count > 0)
                return BadRequest($"Rights named {dataModel.Name} already exists, please choose another name.");
                
            
            rightsRepository.Add(dataModel);

            // _context.RightsDbSet.Add(dataModel);

            // _context.SaveChanges();
            return Ok($"New rights {dataModel.Name} created successfully.");
        }
    }
}