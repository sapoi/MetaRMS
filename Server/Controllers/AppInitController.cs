using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SharedLibrary.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    public class AppInitController : Controller
    {
        private readonly DatabaseContext _context;

        public AppInitController(DatabaseContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult Create(IFormFile file)
        {
            var stringFile = string.Empty;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                stringFile = reader.ReadToEnd();
            }
            AppDataDescriptor fromJsonResult = JsonConvert.DeserializeObject<AppDataDescriptor>(stringFile);
            Application newApplication = new Application {Name = fromJsonResult.AppName, AppDataDescriptor = stringFile};
            _context.ApplicationsDbSet.Add(newApplication);
            _context.SaveChangesAsync();
            Console.WriteLine(newApplication.Name);
            return new NoContentResult();
        }
    }
}