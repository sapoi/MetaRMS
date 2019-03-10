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
    public class SharedAppInitHelper
    {
        public List<Message> ValidateJSONAgainstSchema(JObject jObjectApplicationDescriptor)
        {
            List<Message> messages = new List<Message>();

            // Get JSchema from file with schema
            JSchema schema = JSchema.Parse(ApplicationDescriptorJSONSchema.GetSchema());

            IList<string> validationStringMessages;
            if (!jObjectApplicationDescriptor.IsValid(schema, out validationStringMessages))
            {
                foreach (var stringMessage in validationStringMessages)
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                         0002, 
                                         new List<string>(){ stringMessage }));
                }
            }

            return messages;
        }
        public List<Message> ValidateDescriptor(ApplicationDescriptor applicationDescriptor)
        {
            List<Message> messages = new List<Message>();

            messages.AddRange(validateDatasets(applicationDescriptor));
            messages.AddRange(validateAttributes(applicationDescriptor));

            return messages;
        }

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
        List<Message> validateDatasets(ApplicationDescriptor applicationDescriptor)
        {
            List<Message> messages = new List<Message>();

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
        List<Message> validateOneAttribute(ApplicationDescriptor applicationDescriptor, DatasetDescriptor dataset, AttributeDescriptor attribute, bool isPassword = false)
        {
            var messages = new List<Message>();
            
            // Not a basic type
            if (!AttributeType.Types.Contains(attribute.Type))
            {
                var validReferences = applicationDescriptor.Datasets.Select(d => d.Name).ToList();
                validReferences.Add(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name);
                // Invalid reference
                if (!validReferences.Contains(attribute.Type))
                    messages.Add(new Message(MessageTypeEnum.Error, 
                            0010, 
                            new List<string>(){ dataset.Name, attribute.Name, attribute.Type }));
                // Valid reference
                else
                {
                    // Must have OnDeleteAction
                    if (attribute.OnDeleteAction == OnDeleteActionEnum.None)
                        messages.Add(new Message(MessageTypeEnum.Error, 
                            0011, 
                            new List<string>(){ attribute.Name, dataset.Name }));
                    // And if attribute is of type system users dataset, then the OnDeleteAction cannot be Cascade,
                    // to prevent deleting last user of the application by accident.
                    if (attribute.Type == applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name
                        && attribute.OnDeleteAction == OnDeleteActionEnum.Cascade)
                        messages.Add(new Message(MessageTypeEnum.Error, 
                            0012, 
                            new List<string>(){ attribute.Name, dataset.Name, attribute.Type }));
                    // And if attribute is Required or has Min > 0, then OnDeleteAction cannot be SetEmpty
                    if ((attribute.Required == true || (attribute.Min != null && attribute.Min > 0))
                        && attribute.OnDeleteAction == OnDeleteActionEnum.SetEmpty)
                        messages.Add(new Message(MessageTypeEnum.Error, 
                            0025, 
                            new List<string>(){ attribute.Name, dataset.Name }));
                    // If Min is set, Required cannot be false
                    if (attribute.Min != null && attribute.Required == false)
                        messages.Add(new Message(MessageTypeEnum.Error, 
                            0024, 
                            new List<string>(){ attribute.Name, dataset.Name }));
                }
            }
            // A basic type
            else
            {
                // Password attribute
                if (isPassword) 
                {
                    // Must have its type set to password
                    if (attribute.Type != "password")
                        messages.Add(new Message(MessageTypeEnum.Error, 
                                        0009, 
                                        new List<string>(){ attribute.Name }));
                    // Must be required
                    if (attribute.Required != true)
                        messages.Add(new Message(MessageTypeEnum.Error, 
                                        0014, 
                                        new List<string>(){ attribute.Name }));
                }
                // Not a password
                else
                {
                    // If isPassword flag is not set, attribute type cannot be "password"
                    if (attribute.Type == "password")
                        messages.Add(new Message(MessageTypeEnum.Error, 
                                        0015, 
                                        new List<string>(){ attribute.Name, dataset.Name }));
                }
                // Can not have OnDeleteAction other than None
                if (attribute.OnDeleteAction != OnDeleteActionEnum.None)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                            0013, 
                            new List<string>(){ attribute.Name, dataset.Name }));
            }
            // All types
            // Safer can be set only for password
            if (!isPassword)
                if (attribute.Safer != null)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                    0016, 
                                    new List<string>(){ attribute.Name, dataset.Name }));
            // Min <= Max if both set
            if (attribute.Min != null && attribute.Max != null && attribute.Min > attribute.Max)
                messages.Add(new Message(MessageTypeEnum.Error, 
                            0017, 
                            new List<string>(){ attribute.Name, dataset.Name }));
            // Min > 0
            if (attribute.Min != null && attribute.Min <= 0)
                messages.Add(new Message(MessageTypeEnum.Error, 
                            0018, 
                            new List<string>(){ attribute.Name, dataset.Name }));
            // Max > 0
            if (attribute.Max != null && attribute.Max <= 0)
                messages.Add(new Message(MessageTypeEnum.Error, 
                            0019, 
                            new List<string>(){ attribute.Name, dataset.Name }));

            return messages;
        }
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