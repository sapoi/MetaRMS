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
                return BadRequest("Soubor neni ve spravnem formatu.");
            }
            // check if AppName is unique
            if (_context.ApplicationsDbSet.Where(app => app.Name == applicationDescriptor.AppName).Count() != 0)
            {
                //TODO error
                //ModelState.AddModelError("ErrorCode", "001");
                //return BadRequest(ModelState);
                return BadRequest("Jmeno aplikace jiz existuje.");
            }
            // check if no dataset atribute has name BDId
            if (applicationDescriptor.Datasets.Any(d => d.Attributes.Any(a => a.Name == "DBId")))
                return BadRequest("Nepovoleny nazev atributu DBId");
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
                        Name = applicationDescriptor.AppName, 
                        ApplicationDescriptorJSON = serializedApplicationDescriptor
                        };
                    _context.ApplicationsDbSet.Add(newApplication);
                    // create new admin account for new application and add it to DB
                    string newPassword = PasswordHelper.GenerateRandomPassword(8);
                    RightsModel newRights = getAdminRights(newApplication, applicationDescriptor);
                    _context.RightsDbSet.Add(newRights);
                    UserModel newUser = new UserModel
                    {
                        Application = newApplication,
                        Username = "admin",
                        Password = PasswordHelper.ComputeHash(newPassword),
                        Data = "",
                        Rights = newRights
                    };
                    _context.UsersDbSet.Add(newUser);
                    // try to send login details to admin account to email from parametres
                    sendEmailWithCredentials(email, newPassword);
                    transaction.Commit();
                }
                catch
                {
                    return BadRequest(" ??? ");
                }
            }
            // if everythong was ok, save changes to DB and return Ok
            _context.SaveChangesAsync();
            return Ok();
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
        void sendEmailWithCredentials(string email, string password)
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
            mailMessage.Body = "Username: admin \nPassword: " + password;
            mailMessage.Subject = "Admin login credentials";
            client.Send(mailMessage);
        }
    }
}