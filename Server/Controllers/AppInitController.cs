using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SharedLibrary;
using SharedLibrary.Descriptors;
using SharedLibrary.Models;
using SharedLibrary.Helpers;
using SharedLibrary.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Server.Repositories;

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
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Create(string email, IFormFile file)
        {
            var stringFile = string.Empty;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                stringFile = reader.ReadToEnd();
            }
            ApplicationDescriptor applicationDescriptor;
            try
            {
                applicationDescriptor = JsonConvert.DeserializeObject<ApplicationDescriptor>(stringFile);
            }
            catch
            {
                //TODO detekovat, kde je chyba
                return BadRequest("JSON file is in incorrect format, please read again about page.");
            }
            // check if LoginApplicationName is unique
            var applicationRepository = new ApplicationRepository(_context);
            var applicationModel = applicationRepository.GetByLoginApplicationName(applicationDescriptor.LoginApplicationName);
            if (applicationModel != null)
            {
                //TODO error
                //ModelState.AddModelError("ErrorCode", "001");
                //return BadRequest(ModelState);
                return BadRequest($"Application name {applicationDescriptor.LoginApplicationName} already exists, please choose another.");
            }
            // check if no dataset atribute has name BDId
            if (applicationDescriptor.Datasets.Any(d => d.Attributes.Any(a => a.Name == "DBId")))
                return BadRequest($"Application descriptor contains invalid attribute name \"DBId\"");
                //TODO otestovat
            // add unique Id for each dataset
            for (int i = 0; i < applicationDescriptor.Datasets.Count; i++)
            {
                applicationDescriptor.Datasets[i].Id = i + 1;
            }
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // create new application and add it to DB
                    string serializedApplicationDescriptor = JsonConvert.SerializeObject(applicationDescriptor);
                    ApplicationModel newApplication = new ApplicationModel { 
                        LoginApplicationName = applicationDescriptor.LoginApplicationName, 
                        ApplicationDescriptorJSON = serializedApplicationDescriptor
                        };
                    applicationRepository.Add(newApplication);
                    // _context.ApplicationDbSet.Add(newApplication);
                    // create new admin account for new application and add it to DB
                    string newPassword = PasswordHelper.GenerateRandomPassword(8);
                    RightsModel newRights = getAdminRights(newApplication, applicationDescriptor);
                    var rightsRepository = new RightsRepository(_context);
                    rightsRepository.Add(newRights);
                    // _context.RightsDbSet.Add(newRights);
                    UserModel newUser = new UserModel
                    {
                        Application = newApplication,
                        Password = PasswordHelper.ComputeHash(newPassword),
                        Data = getDefaultAdminDataDictionary(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor),
                        Rights = newRights
                    };
                    var userRepository = new UserRepository(_context);
                    userRepository.Add(newUser);
                    // _context.UserDbSet.Add(newUser);
                    // try to send login details to admin account to email from parametres
                    sendEmailWithCredentials(email, newApplication.LoginApplicationName, newPassword);
                    transaction.Commit();
                }
                catch
                {
                    return BadRequest("Email address is not valid, please choose another.");
                }
            }
            // if everythong was ok, save changes to DB and return Ok
            _context.SaveChangesAsync();
            return Ok($"Application {applicationDescriptor.ApplicationName} was created successfully and login credentials were sent to email {email}.");
        }
        // creates empty user data based on application descriptor
        string getDefaultAdminDataDictionary(UsersDatasetDescriptor usersDatasetDescriptor)
        {
            StringBuilder sb = new StringBuilder("{");
            foreach (var attribute in usersDatasetDescriptor.Attributes)
            {
                // [] bacause it is an empty list
                // if attribute type is usermane, set value to "admin"
                if (attribute.Type == "username")
                    sb.Append($"\"{attribute.Name}\":[\"admin\"],");
                // othewise keep the value list empty
                else
                    sb.Append($"\"{attribute.Name}\":[],");
            }
            // last comma is not a problem for parsing
            sb.Append("}");
            return sb.ToString();
        }
        RightsModel getAdminRights(ApplicationModel appModel, ApplicationDescriptor appDescriptor)
        {
            RightsModel rights = new RightsModel();
            rights.Application = appModel;
            rights.Name = "admin";

            Dictionary<long, RightsEnum> rightsDict = new Dictionary<long, RightsEnum>();
            // key -1 is representing table users
            rightsDict[(long)SystemDatasetsEnum.Users] = RightsEnum.CRUD;
            // key -2 is representing table rights
            rightsDict[(long)SystemDatasetsEnum.Rights] = RightsEnum.CRUD;
            // positive integers are representing datasets
            foreach (var dataset in appDescriptor.Datasets)
            {
                rightsDict[dataset.Id] = RightsEnum.CRUD;
            }
            // serialize rights to JSON
            rights.Data = JsonConvert.SerializeObject(rightsDict);
            return rights;
        }
        void sendEmailWithCredentials(string email, string loginApplicationName, string password)
        {
            var client = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("sapoiapps@gmail.com", "sapoisapoi")
                };

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("sapoiapps@gmail.com");
            mailMessage.To.Add(email);
            mailMessage.Body = $"Application Name: {loginApplicationName} \nUsername: admin \nPassword: {password}";
            mailMessage.Subject = "Admin login credentials";
            client.Send(mailMessage);
        }
    }
}