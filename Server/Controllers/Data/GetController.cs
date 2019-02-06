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
using SharedLibrary.Descriptors;
using Server.Cache;
using Server.Repositories;
using Server.Helpers;

namespace Server.Controllers.Data
{
    [Route("api/data/[controller]")]
    public class GetController : Controller
    {
        private readonly DatabaseContext _context;

        public GetController(DatabaseContext context)
        {
            _context = context;
        }
        ///
        /// if user from HttpContext has at least Read rights to dataset from parameter returns all dataset data
        //TODO handle BadRequest, Forbid, Unauthorized
        ///
        [Authorize]
        [HttpGet]
        [Route("{appName}/{datasetName}")]
        public IActionResult GetAll(string appName, string datasetName)
        {
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var userModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (userModel == null)
                return Unauthorized();
            // Dataset descriptor
            var datasetDescriptor = userModel.Application.ApplicationDescriptor.Datasets.FirstOrDefault(d => d.Name == datasetName);
            if (datasetDescriptor == null)
                return BadRequest($"Dataset name \"{datasetName}\" not found.");
            // Authorization
            if (!controllerHelper.Authorize(userModel, datasetDescriptor.Id, RightsEnum.R))
                return Forbid();
            // Get data from database
            var dataRepository = new DataRepository(_context);
            var query = dataRepository.GetAllByApplicationIdAndDatasetId(userModel.ApplicationId, datasetDescriptor.Id);
            // Prepare data for client
            DataHelper dataHelper = new DataHelper(_context, userModel.Application, datasetDescriptor.Id);
            foreach (var item in query)
                dataHelper.PrepareOneRowForClient(item);
            return Ok(query);


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
            // // var datasetId = adh.GetDatasetIdByName(datasetName);
            // // if (datasetId == null)
            // //     return BadRequest("spatny nazev datasetu");
            // ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptor);
            // var datasetDescriptor = adh.GetDatasetDescriptorByName(datasetName);
            // if (datasetDescriptor == null)
            //     return BadRequest($"ERROR: Dataset name {datasetName} does not exist.");
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
            // // // read user's rights from DB
            // // var rights = (from r in _context.RightsDbSet
            // //               where r.Id == user.RightsId
            // //               select r).FirstOrDefault();
            // // if (rights == null)
            // //     return BadRequest("prava neexistuji");
            // var rightsDict = JsonConvert.DeserializeObject<Dictionary<long, RightsEnum>>(user.Rights.Data);
            // var datasetRights = rightsDict.Where(r => r.Key == datasetDescriptor.Id).FirstOrDefault();
            // if (datasetRights.Equals(default(KeyValuePair<long, RightsEnum>)))
            //     return BadRequest("v pravech neni dataset s datasetId");
            // if (datasetRights.Value <= RightsEnum.None)
            //     return Forbid();
            // var dataRepository = new DataRepository(_context);
            // var query = dataRepository.GetAllByApplicationIdAndDatasetId(application.Id, datasetDescriptor.Id);
            // // List<DataModel> query = (from p in _context.DataDbSet
            // //                          where (p.Application.LoginApplicationName == appName && p.DatasetId == datasetId)
            // //                          select p).ToList();
            // // List<Dictionary<String, List<Object>>> result = new List<Dictionary<String, List<Object>>>();

            // // prepare data for client - add DBId and add text representation for references
            // DataHelper dataHelper = new DataHelper(_context, application, datasetDescriptor.Id);
            // foreach (var item in query)
            // {
            //     dataHelper.PrepareOneRowForClient(item);
            // }
            // // var result = dataHelper.PrepareForClient(query.ToList<BaseModelWithApplicationAndData>(), long.Parse(datasetId.ToString()));
            // return Ok(query);



            // var referenceIndexTypeTuple = new List<(string Name, string Type)>();
            // if (query.Count() > 0)
            //     foreach (var attribute in application.ApplicationDescriptor.Datasets.Where(d => d.Id == datasetId).First().Attributes)
            //     {
            //         bool isReference = !AttributeType.Types.Contains(attribute.Type);
            //         if (isReference)
            //             referenceIndexTypeTuple.Add((attribute.Name, attribute.Type));
            //     }

            // cache for reference so that if one reference is used repeatedly, the DB is queried just once
            // ReferenceCache referenceCache = new ReferenceCache(_context, application);
            // foreach (var row in query)
            // {
            //     var tmpDict = row.DataDictionary;
            //     // add text representation for references
            //     foreach (var attribute in referenceIndexTypeTuple)
            //     {
            //         var tmpIds = tmpDict[attribute.Name];
            //         tmpDict[attribute.Name] = new List<object>();
            //         var lastId = tmpIds.Last();
            //         foreach (var stringId in tmpIds)
            //         {
            //             long id;
            //             if (long.TryParse(stringId.ToString(), out id))
            //             {
            //                 string value = "";
            //                 value += referenceCache.getTextForReference(attribute.Type, id);
            //                 tmpDict[attribute.Name].Add( new Tuple<string, string>(id.ToString(), value) );
            //                 if (!stringId.Equals(lastId))
            //                     value += ", ";
            //             }
            //         }
            //     }
            //     // serializing list containing DBId, because every data is expected to be in a list
            //     tmpDict.Add("DBId", new List<object>() { row.Id } );
            //     result.Add(tmpDict);
            // }
            // return Ok(result);
        }
        [Authorize]
        [HttpGet]
        [Route("{appName}/{datasetName}/{id}")]
        public IActionResult GetById(string appName, string datasetName, long id)
        {
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            // Dataset descriptor
            var datasetDescriptor = authUserModel.Application.ApplicationDescriptor.Datasets.FirstOrDefault(d => d.Name == datasetName);
            if (datasetDescriptor == null)
                return BadRequest($"Dataset name \"{datasetName}\" not found.");
            // Authorization
            if (!controllerHelper.Authorize(authUserModel, datasetDescriptor.Id, RightsEnum.R))
                return Forbid();
            // Get data from database
            var dataRepository = new DataRepository(_context);
            var dataModel = dataRepository.GetById(authUserModel.ApplicationId, datasetDescriptor.Id, id);
            if (dataModel == null)
                return BadRequest($"No element data with id {id} found in dataset \"{datasetName}\".");
            // Prepare data for client
            DataHelper dataHelper = new DataHelper(_context, authUserModel.Application, datasetDescriptor.Id);
            dataHelper.PrepareOneRowForClient(dataModel);
            return Ok(dataModel);


            // var applicationRepository = new ApplicationRepository(_context);
            // var application = applicationRepository.GetByLoginApplicationName(appName);
            // // ApplicationModel application = (from a in _context.ApplicationDbSet
            // //                                 where (a.LoginApplicationName == appName)
            // //                                 select a).FirstOrDefault();
            // if (application == null)
            //     return BadRequest("spatny nazev aplikace neexistuje");
            // // ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptorJSON);
            // // var datasetId = adh.GetDatasetIdByName(datasetName);
            // // if (datasetId == null)
            // //     return BadRequest("spatny nazev datasetu");
            // ApplicationDescriptorHelper adh = new ApplicationDescriptorHelper(application.ApplicationDescriptor);
            // var datasetDescriptor = adh.GetDatasetDescriptorByName(datasetName);
            // if (datasetDescriptor == null)
            //     return BadRequest($"ERROR: Dataset name {datasetName} does not exist.");
            // var dataRepository = new DataRepository(_context);
            // //TODO kontrolovat jeste dataset+aplikaci
            // var query = dataRepository.GetById(id);
            // // DataModel query = (from p in _context.DataDbSet
            // //                    where (p.Application.LoginApplicationName == appName && p.DatasetId == datasetId && p.Id == id)
            // //                    select p).FirstOrDefault();
            // if (query == null)
            //     return BadRequest("neexistujici kombinace jmena aplikace, datasetu a id");
            // return Ok(query.DataDictionary);
        }
    }
}