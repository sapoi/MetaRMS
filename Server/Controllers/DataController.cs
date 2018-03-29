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
    public class DataController : Controller
    {
        private readonly DatabaseContext _context;

        public DataController(DatabaseContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult Create(string appName, string dataset, string data)
        {
            Data d = new Data{ ApplicationName = appName, DatasetName = dataset, JsonData = data };
            d.LoadFromJson();
            foreach (var item in d.data)
            {
                Console.WriteLine(item.Key + " : " + item.Value);
            }
            _context.DataDbSet.Add(d);
            _context.SaveChangesAsync();
            Console.WriteLine(d.Id);
            return new NoContentResult();
        }
    }
}