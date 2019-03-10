using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using SharedLibrary.Models;
using SharedLibrary.Helpers;
using SharedLibrary.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using RazorWebApp.Repositories;
using SharedLibrary.Structures;
using Newtonsoft.Json.Linq;

namespace RazorWebApp.Controllers
{
    [Route("api/[controller]")]
    public class AppInitController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;

        public AppInitController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for creating new application.
        /// </summary>
        /// <param name="email">Email address to send the login data to</param>
        /// <param name="file">File containing application descriptor in JSON format</param>
        /// <returns>Messages about action result</returns>
        /// <response code="200">If application successfully created</response>
        /// <response code="404">If input is not valid</response>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult Create(string email, IFormFile file)
        {
            // List of messages to return to the client
            var messages = new List<Message>();

            #region application descriptor validations

            // File with JSON application descriptor is required
            if (file == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                         0001, 
                                         new List<string>()));
                return BadRequest(messages);
            }

            // Get JObject from input file
            JObject applicationDescriptorJObject;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                // Try to parse file to JObject - only valid JSON files are parsed
                try 
                {
                    applicationDescriptorJObject = JObject.Parse(reader.ReadToEnd());
                }
                // If parsing was unsuccessfull, return error message containing location of error
                catch (JsonReaderException e)
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                         0002, 
                                         new List<string>(){ e.Message }));
                    return BadRequest(messages);
                }
            }
            var appInitHelper = new AppInitHelper();

            // With successfully parsed JSON file, validate it against schema
            var schemaValidationMessages = appInitHelper.ValidateJSONAgainstSchema(applicationDescriptorJObject);
            // If validation JSON is not valid return errors
            if (schemaValidationMessages.Count != 0)
                return BadRequest(schemaValidationMessages);
            // Get ApplicationDescriptor class instance from JObject
            var applicationDescriptor = applicationDescriptorJObject.ToObject<ApplicationDescriptor>();

            // LoginApplicationName must be unique
            var applicationRepository = new ApplicationRepository(context);
            var applicationModel = applicationRepository.GetByLoginApplicationName(applicationDescriptor.LoginApplicationName);
            if (applicationModel != null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                         0003, 
                                         new List<string>(){ applicationDescriptor.LoginApplicationName }));
            }
            // Validate datasets and attributes
            messages.AddRange(appInitHelper.ValidateDescriptor(applicationDescriptor));

            if (messages.Count != 0)
                return BadRequest(messages);

            #endregion

            // Set default values to the application descriptor
            appInitHelper.SetDefaultDescriptorValues(applicationDescriptor);

            #region create new application

            using (var transaction = context.Database.BeginTransaction())
            {
                // Create new application and add it to the database
                var serializedApplicationDescriptor = JsonConvert.SerializeObject(applicationDescriptor);
                var newApplication = new ApplicationModel { 
                    LoginApplicationName = applicationDescriptor.LoginApplicationName, 
                    ApplicationDescriptorJSON = serializedApplicationDescriptor
                    };
                applicationRepository.Add(newApplication);

                // Create new admin account for the application
                // Random password 8 chars long
                var newPassword = PasswordHelper.GenerateRandomPassword(8);
                // Admin rights
                var newRights = getAdminRights(newApplication, applicationDescriptor);
                var rightsRepository = new RightsRepository(context);
                rightsRepository.Add(newRights);
                var newUser = new UserModel
                {
                    Application = newApplication,
                    Password = PasswordHelper.ComputeHash(newPassword),
                    Data = getDefaultAdminDataDictionary(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor),
                    Rights = newRights
                };
                var userRepository = new UserRepository(context);
                userRepository.Add(newUser);

                // Try to send login details to admin account to email from parametres
                try
                {
                    sendEmailWithCredentials(email, applicationDescriptor.ApplicationName, newApplication.LoginApplicationName, newPassword);
                }
                catch
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                0026, 
                                new List<string>(){ email }));
                    return BadRequest(messages);
                }

                // Commit all 
                transaction.Commit();
            }
            // If everythong was ok, save changes to the database
            context.SaveChangesAsync();

            #endregion

            messages.Add(new Message(MessageTypeEnum.Error, 
                                0027, 
                                new List<string>(){ applicationDescriptor.ApplicationName, email }));
            return Ok(messages);
        }
        /// <summary>
        /// Creates a JSON formatted string of admin data containing username admin and wit all the other attributes blank
        /// </summary>
        /// <param name="usersDatasetDescriptor">User dataset descriptor of the new application</param>
        /// <returns>Default user data for admin user</returns>
        string getDefaultAdminDataDictionary(UsersDatasetDescriptor usersDatasetDescriptor)
        {
            // Build a string based on system user dataset descriptor
            StringBuilder sb = new StringBuilder("{");
            foreach (var attribute in usersDatasetDescriptor.Attributes)
            {
                // If attribute type is usermane, set value to "admin"
                if (attribute.Type == "username")
                    sb.Append($"\"{attribute.Name}\":[\"admin\"],");
                // Othewise keep the value list empty
                else
                    sb.Append($"\"{attribute.Name}\":[],");
            }
            // Last comma is not a problem for parsing
            sb.Append("}");
            return sb.ToString();
        }
        /// <summary>
        /// Returns default rights for admin
        /// </summary>
        /// <param name="applicationModel">Model of application the rights belongs to</param>
        /// <param name="applicationDescriptor">Descriptor of the application</param>
        /// <returns>Admin RightsModel</returns>
        RightsModel getAdminRights(ApplicationModel applicationModel, ApplicationDescriptor applicationDescriptor)
        {
            var rights = new RightsModel();
            rights.Application = applicationModel;
            rights.Name = "admin";

            // Full CRUD rights for all datasets
            Dictionary<long, RightsEnum> rightsDict = new Dictionary<long, RightsEnum>();
            rightsDict[(long)SystemDatasetsEnum.Users] = RightsEnum.CRUD;
            rightsDict[(long)SystemDatasetsEnum.Rights] = RightsEnum.CRUD;
            foreach (var dataset in applicationDescriptor.Datasets)
            {
                rightsDict[dataset.Id] = RightsEnum.CRUD;
            }

            // Serialize rights to JSON
            rights.Data = JsonConvert.SerializeObject(rightsDict);
            return rights;
        }
        /// <summary>
        /// Sends confirmation email about application creation to provided email address
        /// </summary>
        /// <param name="email">Email address to end the email to</param>
        /// <param name="applicationName">Name of the application</param>
        /// <param name="loginApplicationName">Login name of the application</param>
        /// <param name="password">Admin password</param>
        void sendEmailWithCredentials(string email, string applicationName, string loginApplicationName, string password)
        {
            // Create SMTP client
            var client = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("sapoiapps@gmail.com", "sapoisapoi")
                };

            // Create message
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("sapoiapps@gmail.com");
            mailMessage.To.Add(email);
            mailMessage.Body = $"Application Name: {loginApplicationName} \nUsername: admin \nPassword: {password}";
            mailMessage.Subject = $"{applicationName} admin login credentials";

            // Send email
            client.Send(mailMessage);
        }
    }
}