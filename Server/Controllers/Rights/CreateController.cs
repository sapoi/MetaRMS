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
        public IActionResult CreateById(string appName, [FromBody] RightsModel fromBodyRightsModel)
        {
            ApplicationModel application = (from a in _context.ApplicationsDbSet
                                   where (a.Name == appName)
                                   select a).FirstOrDefault();
            if (application == null)
                return BadRequest("spatny nazev aplikace neexistuje");
            // ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            // // separate rights name from data
            // string rightsName = "";
            // Dictionary<string, object> rightsData = new Dictionary<string, object>();
            // foreach (var item in fromBodyData)
            // {
            //     if(item.Key == "Name")
            //         rightsName = item.Value.ToString();
            //     else
            //     {
            //         rightsData.Add(item.Key, item.Value);
            //     }
            // }
            string JsonData = JsonConvert.SerializeObject(fromBodyRightsModel.DataDictionary);
            
            //TODO posilat rights name
            RightsModel dataModel = new RightsModel{ApplicationId=application.Id, Name=fromBodyRightsModel.Name, Data=JsonData};
            _context.RightsDbSet.Add(dataModel);

            _context.SaveChanges();
            return Ok("saved successfully");
        }
    }
}