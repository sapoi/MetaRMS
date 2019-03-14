using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.StaticFiles;
using SharedLibrary.Structures;

namespace SharedLibrary.Helpers
{
    /// <summary>
    /// SharedAppInitHelper can be used by any application to validate application descriptor in JSON fromat.
    /// </summary>
    public class SharedAppInitHelper
    {
        /// <summary>
        /// This method validates application descriptor agains schema
        /// </summary>
        /// <param name="jObjectApplicationDescriptor">Aplication descriptor in JObject format</param>
        /// <returns>List of messages</returns>
        public List<Message> ValidateJSONAgainstSchema(JObject jObjectApplicationDescriptor)
        {
            List<Message> messages = new List<Message>();

            // Get JSchema from file with schema
            JSchema schema = JSchema.Parse(ApplicationDescriptorJSONSchema.GetSchema());

            IList<string> validationStringMessages;
            if (!jObjectApplicationDescriptor.IsValid(schema, out validationStringMessages))
            {
                // For each string message from validation against schema create Message class instance
                foreach (var stringMessage in validationStringMessages)
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                         0002, 
                                         new List<string>(){ stringMessage }));
                }
            }
            return messages;
        }
        /// <summary>
        /// This method validates application datasets and their attributes
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor to validate</param>
        /// <returns>List of messages</returns>
        public List<Message> ValidateDescriptor(ApplicationDescriptor applicationDescriptor)
        {
            List<Message> messages = new List<Message>();

            messages.AddRange(validateDatasets(applicationDescriptor));
            messages.AddRange(validateAttributes(applicationDescriptor));

            return messages;
        }
        /// <summary>
        /// This method sets default values to the application descriptor.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor to set the values to</param>
        public void SetDefaultDescriptorValues(ApplicationDescriptor applicationDescriptor)
        {
            // Add unique Id for each dataset
            for (int i = 0; i < applicationDescriptor.Datasets.Count; i++)
            {
                applicationDescriptor.Datasets[i].Id = i + 1;
            }
            // Add id -1 to users dataset
            applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Id = (long)SystemDatasetsEnum.Users;
            // Defalut values to attributes properties
            var allAttributeDescriptors = applicationDescriptor.Datasets.SelectMany(d => d.Attributes).ToList();
            allAttributeDescriptors.AddRange(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes);
            allAttributeDescriptors.Add(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.PasswordAttribute);
            foreach (var attributeDescriptor in allAttributeDescriptors)
            {
                // Relation between required and min for references
                if (!AttributeType.Types.Contains(attributeDescriptor.Type))
                {
                    // If min is set and Required is not set
                    if (attributeDescriptor.Min != null && attributeDescriptor.Required == null)
                        attributeDescriptor.Required = true;
                    // If required is set and min is not set
                    else if (attributeDescriptor.Required != null && attributeDescriptor.Min == null)
                        attributeDescriptor.Min = 1;
                }
                // If Required or Unique properties are not set, set them to false
                if (attributeDescriptor.Required == null)
                    attributeDescriptor.Required = false;
                if (attributeDescriptor.Unique == null)
                    attributeDescriptor.Unique = false;
            }
            // If password Safer property is not set, set it to false
            if (applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.PasswordAttribute.Safer == null)
                applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.PasswordAttribute.Safer = false;
        }
        /// <summary>
        /// This method validates application datasets.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor to validate</param>
        /// <returns>List of messages</returns>
        List<Message> validateDatasets(ApplicationDescriptor applicationDescriptor)
        {
            List<Message> messages = new List<Message>();

            // Get all datasets
            var datasets = new List<DatasetDescriptor>();
            datasets.AddRange(applicationDescriptor.Datasets);
            datasets.Add(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor);
            // Dataset names must be unique within one application, so it can be referenced
            var duplicateDatasetNames = datasets.GroupBy(d => d.Name).Where(g => g.Count() > 1);
            foreach (var datasetGroup in duplicateDatasetNames)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                0004, 
                                new List<string>(){ datasetGroup.Key, datasetGroup.Count().ToString() }));
            }
            // Dataset names cannot be the same as basic type names, so it can be used as reference
            var datasetsNamedAsBasicTypes = datasets.Where(d => AttributeType.Types.Contains(d.Name));
            foreach (var dataset in datasetsNamedAsBasicTypes)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                0005, 
                                new List<string>(){ dataset.Name, string.Join(", ", AttributeType.Types) }));
            }
            // Dataset names cannot contain {number}, because of error messages placeholders
            var datasetsNamedAsPlaceholder = datasets.Where(d => Regex.IsMatch(d.Name, "{[0-9]*}"));
            foreach (var dataset in datasetsNamedAsPlaceholder)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                0006, 
                                new List<string>(){ dataset.Name }));
            }
            // At least one required attribute in each dataset
            var datasetsWithoutRequiredAttribute = datasets.Where(d => !d.Attributes.Any(a => a.Required == true));
            foreach (var dataset in datasetsWithoutRequiredAttribute)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                0007, 
                                new List<string>(){ dataset.Name }));
            }
            // Attributes within dataset must have unique names
            foreach (var dataset in datasets)
            {
                var duplicateAttributeNames = dataset.Attributes.GroupBy(a => a.Name).Where(g => g.Count() > 1);
                foreach (var attributeGroup in duplicateAttributeNames)
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                0008, 
                                new List<string>(){ dataset.Name, attributeGroup.Key, attributeGroup.Count().ToString() }));
                }
            }

            return messages;
        }
        /// <summary>
        /// This method validates application attributes.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor to validate</param>
        /// <returns>List of messages</returns>
        List<Message> validateAttributes(ApplicationDescriptor applicationDescriptor)
        {
            List<Message> messages = new List<Message>();

            // Attributes of user defined datasets
            foreach (var dataset in applicationDescriptor.Datasets)
            {
                foreach (var attribute in dataset.Attributes)
                {
                    messages.AddRange(validateOneAttribute(applicationDescriptor, dataset, attribute));
                }
            }
            // Attributes of system users dataset
            foreach (var attribute in applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes)
            {
                messages.AddRange(validateOneAttribute(applicationDescriptor, applicationDescriptor.SystemDatasets.UsersDatasetDescriptor, attribute));
            }
            // Password attribute
            messages.AddRange(validateOneAttribute(applicationDescriptor, applicationDescriptor.SystemDatasets.UsersDatasetDescriptor, 
                                                   applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.PasswordAttribute, true));
            // Username validation
            messages.AddRange(usernameValidations(applicationDescriptor));

            return messages;
        }
        /// <summary>
        /// This method validates single application attribute.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor to validate</param>
        /// <param name="datasetDescriptor">Dataset the attribute is from</param>
        /// <param name="attributeDescriptor">Attribute to validate</param>
        /// <param name="isPassword">True if attribute is a password attribute</param>
        /// <returns>List of messages</returns>
        List<Message> validateOneAttribute(ApplicationDescriptor applicationDescriptor, DatasetDescriptor datasetDescriptor, AttributeDescriptor attributeDescriptor, bool isPassword = false)
        {
            var messages = new List<Message>();
            
            // Not a basic type
            if (!AttributeType.Types.Contains(attributeDescriptor.Type))
            {
                var validReferences = applicationDescriptor.Datasets.Select(d => d.Name).ToList();
                validReferences.Add(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name);
                // Invalid reference
                if (!validReferences.Contains(attributeDescriptor.Type))
                    messages.Add(new Message(MessageTypeEnum.Error, 
                            0010, 
                            new List<string>(){ datasetDescriptor.Name, attributeDescriptor.Name, attributeDescriptor.Type }));
                // Valid reference
                else
                {
                    // Must have OnDeleteAction
                    if (attributeDescriptor.OnDeleteAction == OnDeleteActionEnum.None)
                        messages.Add(new Message(MessageTypeEnum.Error, 
                            0011, 
                            new List<string>(){ attributeDescriptor.Name, datasetDescriptor.Name }));
                    // And if attribute is in system users dataset, then the OnDeleteAction cannot be Cascade,
                    // to prevent deleting last user of the application by accident.
                    if (datasetDescriptor == applicationDescriptor.SystemDatasets.UsersDatasetDescriptor
                        && attributeDescriptor.OnDeleteAction == OnDeleteActionEnum.Cascade)
                        messages.Add(new Message(MessageTypeEnum.Error, 
                            0012, 
                            new List<string>(){ attributeDescriptor.Name, datasetDescriptor.Name, attributeDescriptor.Type }));
                    // And if attribute is Required or has Min > 0, then OnDeleteAction cannot be SetEmpty
                    if ((attributeDescriptor.Required == true || (attributeDescriptor.Min != null && attributeDescriptor.Min > 0))
                        && attributeDescriptor.OnDeleteAction == OnDeleteActionEnum.SetEmpty)
                        messages.Add(new Message(MessageTypeEnum.Error, 
                            0025, 
                            new List<string>(){ attributeDescriptor.Name, datasetDescriptor.Name }));
                    // If Min is set, Required cannot be false
                    if (attributeDescriptor.Min != null && attributeDescriptor.Required == false)
                        messages.Add(new Message(MessageTypeEnum.Error, 
                            0024, 
                            new List<string>(){ attributeDescriptor.Name, datasetDescriptor.Name }));
                }
            }
            // A basic type
            else
            {
                // Password attribute
                if (isPassword) 
                {
                    // Must have its type set to password
                    if (attributeDescriptor.Type != "password")
                        messages.Add(new Message(MessageTypeEnum.Error, 
                                        0009, 
                                        new List<string>(){ attributeDescriptor.Name }));
                    // Must be required
                    if (attributeDescriptor.Required != true)
                        messages.Add(new Message(MessageTypeEnum.Error, 
                                        0014, 
                                        new List<string>(){ attributeDescriptor.Name }));
                }
                // Not a password
                else
                {
                    // If isPassword flag is not set, attribute type cannot be "password"
                    if (attributeDescriptor.Type == "password")
                        messages.Add(new Message(MessageTypeEnum.Error, 
                                        0015, 
                                        new List<string>(){ attributeDescriptor.Name, datasetDescriptor.Name }));
                }
                // Can not have OnDeleteAction other than None
                if (attributeDescriptor.OnDeleteAction != OnDeleteActionEnum.None)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                            0013, 
                            new List<string>(){ attributeDescriptor.Name, datasetDescriptor.Name }));
            }
            // All types
            // Safer can be set only for password
            if (!isPassword)
                if (attributeDescriptor.Safer != null)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                    0016, 
                                    new List<string>(){ attributeDescriptor.Name, datasetDescriptor.Name }));
            // Min <= Max if both set
            if (attributeDescriptor.Min != null && attributeDescriptor.Max != null && attributeDescriptor.Min > attributeDescriptor.Max)
                messages.Add(new Message(MessageTypeEnum.Error, 
                            0017, 
                            new List<string>(){ attributeDescriptor.Name, datasetDescriptor.Name }));
            // Min > 0
            if (attributeDescriptor.Min != null && attributeDescriptor.Min <= 0)
                messages.Add(new Message(MessageTypeEnum.Error, 
                            0018, 
                            new List<string>(){ attributeDescriptor.Name, datasetDescriptor.Name }));
            // Max > 0
            if (attributeDescriptor.Max != null && attributeDescriptor.Max <= 0)
                messages.Add(new Message(MessageTypeEnum.Error, 
                            0019, 
                            new List<string>(){ attributeDescriptor.Name, datasetDescriptor.Name }));

            return messages;
        }
        /// <summary>
        /// This method validates username.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor to validate</param>
        /// <returns>List of messages</returns>
        List<Message> usernameValidations(ApplicationDescriptor applicationDescriptor)
        {
            var messages = new List<Message>();
            
            // No attribute in user define datasets can be of type "username"
            foreach (var datasetDescriptor in applicationDescriptor.Datasets)
            {
                foreach (var usernameTypeAttribute in datasetDescriptor.Attributes.Where(a => a.Type == "username"))
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                0020, 
                                new List<string>(){ usernameTypeAttribute.Name, datasetDescriptor.Name }));
                }
            }
            // There must be exactly one attribute of type "username" in system users dataset
            var systemUsersUsernameTypeAttributes = applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes.Where(a => a.Type == "username");
            if (systemUsersUsernameTypeAttributes.Count() != 1)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                0021, 
                                new List<string>(){ applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name, systemUsersUsernameTypeAttributes.Count().ToString() }));
                return messages;
            }
            // Username attribute must be required
            var usernameAttribute = systemUsersUsernameTypeAttributes.First();
            if (usernameAttribute.Required != true)
                messages.Add(new Message(MessageTypeEnum.Error, 
                                0022, 
                                new List<string>(){ usernameAttribute.Name, applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name }));
            // Username attribute must be unique
            if (usernameAttribute.Required != true)
                messages.Add(new Message(MessageTypeEnum.Error, 
                                0023, 
                                new List<string>(){ usernameAttribute.Name, applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name }));

            return messages;
        }
    }
}