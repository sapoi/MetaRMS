using System;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using System.Linq;
using SharedLibrary.Helpers;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize]
        [HttpPost]
        public IActionResult Create(string appName, string datasetName, string data)
        {
            var application = (from p in _context.ApplicationDbSet
                                 where p.Name == appName
                                 select p).FirstOrDefault();
            if (application == null)
                return BadRequest(); // aplikace nenalazena
            ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            var datasetId = adh.GetDatasetIdByName(datasetName);
            if (datasetId == null)
                return BadRequest(); // dataset nenalazen
            DataModel d = new DataModel{ Application = application, DatasetId = (long)datasetId, Data = data };
            //d.LoadFromJson();
            foreach (var item in d.DataDictionary)
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