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
using Core.Helpers;
using Core.Repositories;
using SharedLibrary.Structures;
using Newtonsoft.Json.Linq;
using SharedLibrary.StaticFiles;

namespace Core.Controllers
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
            var sharedAppInitHelper = new SharedAppInitHelper();

            // With successfully parsed JSON file, validate it against schema
            var schemaValidationMessages = sharedAppInitHelper.ValidateJSONAgainstSchema(applicationDescriptorJObject);
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
            messages.AddRange(sharedAppInitHelper.ValidateDescriptor(applicationDescriptor));

            if (messages.Count != 0)
                return BadRequest(messages);

            #endregion

            // Set default values to the application descriptor
            sharedAppInitHelper.SetDefaultDescriptorValues(applicationDescriptor);

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
                // Random password
                string newPassword;
                var minPasswordLength = applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.PasswordAttribute.Min;
                if (minPasswordLength != null)
                    newPassword = PasswordHelper.GenerateRandomPassword((int)minPasswordLength);
                else
                    newPassword = PasswordHelper.GenerateRandomPassword(Constants.MinSaferPasswordLength);
                // Admin rights
                var appInitHelper = new AppInitHelper();
                var newRights = appInitHelper.GetAdminRights(newApplication, applicationDescriptor);
                var rightsRepository = new RightsRepository(context);
                rightsRepository.Add(newRights);
                var salt = PasswordHelper.GetSalt();
                var newUser = new UserModel
                {
                    Application = newApplication,
                    PasswordHash = PasswordHelper.ComputeHash(salt + newPassword),
                    PasswordSalt = salt,
                    Data = appInitHelper.GetDefaultAdminDataDictionary(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor),
                    Rights = newRights,
                    Language = applicationDescriptor.DefaultLanguage
                };
                var userRepository = new UserRepository(context);
                userRepository.Add(newUser);

                // Try to send login details to admin account to email from parametres
                try
                {
                    appInitHelper.SendEmailWithCredentials(email, applicationDescriptor.ApplicationName, newApplication.LoginApplicationName, newPassword);
                }
                catch
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                0025, 
                                new List<string>(){ email }));
                    return BadRequest(messages);
                }

                // Commit all 
                transaction.Commit();
            }
            // If everythong was ok, save changes to the database
            context.SaveChangesAsync();

            #endregion

            messages.Add(new Message(MessageTypeEnum.Info, 
                                0026, 
                                new List<string>(){ applicationDescriptor.ApplicationName, email }));
            return Ok(messages);
        }
    }
}