using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;

namespace RazorWebApp.Helpers
{
    public class AppInitHelper
    {
        /// <summary>
        /// Creates a JSON formatted string of admin data containing username admin and wit all the other attributes blank
        /// </summary>
        /// <param name="usersDatasetDescriptor">User dataset descriptor of the new application</param>
        /// <returns>Default user data for admin user</returns>
        public string GetDefaultAdminDataDictionary(UsersDatasetDescriptor usersDatasetDescriptor)
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
        public RightsModel GetAdminRights(ApplicationModel applicationModel, ApplicationDescriptor applicationDescriptor)
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
        public void SendEmailWithCredentials(string email, string applicationName, string loginApplicationName, string password)
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
            mailMessage.Subject = $"MetaRMS - {applicationName} admin login credentials";

            // Send email
            client.Send(mailMessage);
        }
    }
}