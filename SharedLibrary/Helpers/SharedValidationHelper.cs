using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using SharedLibrary.Structures;
using SharedLibrary.StaticFiles;

namespace SharedLibrary.Helpers
{
    /// <summary>
    /// The SharedValidationHelper class contains helper methods for input validations by application descriptor.
    /// </summary>
    public class SharedValidationHelper
    {
        /// <summary>
        /// Returns true if password fulfills safety requirements defined by the regular expression
        /// from http://html5pattern.com/Passwords (at least number of characters defined in the
        /// Constants static file characters long and contain at least 1 upper and 1 lower-case letter 
        /// and 1 number or special character)
        /// </summary>
        /// <param name="password">String password to be validated</param>
        /// <returns>True if password is safe, otherwise false</returns>
        public bool IsPasswordSafer(string password)
        {
            return Regex.IsMatch(password, Constants.SaferPasswordPattern);
        }
        /// <summary>
        /// This method validates if the two ids from parametres are equal.
        /// </summary>
        /// <param name="toCheckApplicationId">Received application id</param>
        /// <param name="authUserApplicationId">Application id of authenticated user</param>
        /// <returns>True if id are equal, false otherwise.</returns>
        public List<Message> ValidateApplicationId(long toCheckApplicationId, long authUserApplicationId)
        {
            var messages = new List<Message>();
            if (toCheckApplicationId != authUserApplicationId)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                                  6014, 
                                                  new List<string>(){ toCheckApplicationId.ToString(),
                                                                      authUserApplicationId.ToString() 
                                                                    }
                                                  )
                            );
                Logger.LogMessagesToConsole(messages);
            }
            return messages;
        }
        /// <summary>
        /// This method validates rights dictionary.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor the rights belongs to</param>
        /// <param name="rightsDictionary">Rights data dictionary</param>
        /// <returns>List of messages.</returns>
        public List<Message> ValidateRights(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rightsDictionary)
        {
            // Check that all oligatory rights are filled and are in a correct format
            var messages = validateRightsData(applicationDescriptor, rightsDictionary);
            // Only if the rights are correct, check the logical validity
            if (messages.Count == 0)
                messages.AddRange(validateRightsLogic(applicationDescriptor, rightsDictionary));
            return messages;
        }
        /// <summary>
        /// This method checks that rights dictionary contains rights for all application datasets 
        /// and that the rights are in a correct format.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor the rights belongs to</param>
        /// <param name="rightsDictionary">Rights data dictionary</param>
        /// <returns>List of messages.</returns>
        List<Message> validateRightsData(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rightsDictionary)
        {
            var messages = new List<Message>();
            // Check user-defined datasets
            var datasetsToCheck = new List<DatasetDescriptor>();
            datasetsToCheck.AddRange(applicationDescriptor.Datasets);
            // And users dataset
            datasetsToCheck.Add(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor);
            // And also rights for rights need to be checked by creating mock dataset
            datasetsToCheck.Add(new DatasetDescriptor(){
                                                         Id = (long)SystemDatasetsEnum.Rights, 
                                                         Name = SystemDatasetsEnum.Rights.ToString()
                                                        });
            foreach (var datasetDescriptor in datasetsToCheck)
            {
                var rightsForDataset = rightsDictionary.FirstOrDefault(r => r.Key == datasetDescriptor.Id);
                // If no rights for dataset found
                if (rightsForDataset.Equals(new KeyValuePair<string, List<long>>()))
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6011, 
                                                      new List<string>(){ datasetDescriptor.Name })
                                );
                else if (!Enum.IsDefined(typeof(RightsEnum), rightsForDataset.Value))
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6012, 
                                                      new List<string>(){ datasetDescriptor.Name,
                                                                          rightsForDataset.Value.ToString()
                                                                        })
                                );
            }
            return messages;
        }
        /// <summary>
        /// This method validates that referenced datasets in dataset with rights at least read have also at least read rights.
        /// </summary>
        /// <param name="applicationDescriptor">Application descriptor the rights belongs to</param>
        /// <param name="rightsDictionary">Rights data dictionary</param>
        /// <returns>List of messages.</returns>
        List<Message> validateRightsLogic(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rightsDictionary)
        {
            var messages = new List<Message>();
            // Check user-defined datasets
            var datasetsToCheck = new List<DatasetDescriptor>();
            datasetsToCheck.AddRange(applicationDescriptor.Datasets);
            // And users dataset
            datasetsToCheck.Add(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor);
            // Dictionary containing <dataset id, dataset name>
            var idNameDictionary = datasetsToCheck.ToDictionary(d => d.Name, d => d.Id);
            // Check that for each dataset with at least read right, all datasets referenced in it have at least read rights
            foreach (var datasetDescriptor in datasetsToCheck)
            {
                if (rightsDictionary[datasetDescriptor.Id] >= RightsEnum.R)
                {
                    foreach (var attribute in datasetDescriptor.Attributes)
                    {
                        // If attribute type is reference
                        if (!AttributeType.Types.Contains(attribute.Type))
                        {
                            // And rights for the reference are less than read rights
                            if (rightsDictionary[idNameDictionary[attribute.Type]] < RightsEnum.R)
                            {
                                messages.Add(new Message(MessageTypeEnum.Error, 
                                                                6013, 
                                                                new List<string>(){ 
                                                                                    datasetDescriptor.Name,
                                                                                    attribute.Type
                                                                                    })
                                            );
                            }
                        }
                    }
                }
            }
            return messages;
        }
        /// <summary>
        /// This method validates dataset's data dictionary against the dataset descriptor
        /// </summary>
        /// <param name="datasetDescriptor">Descriptor of dataset the data to validate are from</param>
        /// <param name="dataDictionary">Data  dictionary to validate</param>
        /// <param name="validReferencesIdsDictionary">Dictionary of valid references</param>
        /// <returns>List of messages.</returns>
        public List<Message> ValidateDataByApplicationDescriptor(DatasetDescriptor datasetDescriptor, Dictionary<string, List<object>> dataDictionary, Dictionary<string, List<long>> validReferencesIdsDictionary)
        {
            var messages = new List<Message>();

            // Validate each item in data dictionary
            foreach (var item in dataDictionary)
            {
                // Get item's attribute descriptor
                var attributeDescriptor = datasetDescriptor.Attributes.FirstOrDefault(a => a.Name == item.Key);
                if (attributeDescriptor == null)
                {
                    var attributeNotFoundMessage = new Message(
                        MessageTypeEnum.Error, 
                        6001, 
                        new List<string>(){item.Key, datasetDescriptor.Name},
                        item.Key
                        );
                    Logger.LogMessageToConsole(attributeNotFoundMessage);
                    messages.Add(attributeNotFoundMessage);
                    continue;
                }
                // Validate the attribute
                var attributeMessages = validateOneAttribute(attributeDescriptor, item.Value, validReferencesIdsDictionary);
                if (attributeMessages != null)
                    messages.AddRange(attributeMessages);
            }
            return messages;
        }
        /// <summary>
        /// This method validates a single one item in data dictionary against its attribute descriptor.
        /// </summary>
        /// <param name="attributeDescriptor">Descriptor of attribute the data to validate are from</param>
        /// <param name="dataDictionaryValues">Data dictionary values to validate</param>
        /// <param name="validReferencesIdsDictionary">Dictionary of valid references</param>
        /// <returns>List of messages.</returns>
        List<Message> validateOneAttribute(AttributeDescriptor attributeDescriptor, List<object> dataDictionaryValues, Dictionary<string, List<long>> validReferencesIdsDictionary)
        {
            var messages = new List<Message>();

            // Required
            // If attribute is required and there are no input data, or they are empty or the first element is empty return error message
            if (attributeDescriptor.Required == true && (dataDictionaryValues == null || dataDictionaryValues.Count == 0 || dataDictionaryValues[0].ToString() == ""))
            {
                var message = new Message(MessageTypeEnum.Error, 
                                                   6002, 
                                                   new List<string>(){ attributeDescriptor.Name }, 
                                                   attributeDescriptor.Name);
                messages.Add(message);
                return messages;
            }
            // If the attribute is not required and is empty, no validation is needed, so return empty messages
            if (dataDictionaryValues == null || dataDictionaryValues.Count == 0 || dataDictionaryValues[0].ToString() == "")
                return messages;

            // General data types later used for Min and Max validation
            bool isNumber = false;
            double parsedNumber = 0;
            bool isText = false;
            bool isReference = false;

            // Type
            bool hasTypeError = false;
            switch (attributeDescriptor.Type)
            {
                case "color":
                    if (!Regex.Match(dataDictionaryValues[0].ToString(), "^#(?:[0-9a-fA-F]{3}){1,2}$").Success)
                        hasTypeError = true;
                    break;
                case "date":
                    try {
                        DateTime parsedDate = DateTime.Parse(dataDictionaryValues[0].ToString());
                    }
                    catch (FormatException) {
                        hasTypeError = true;
                    }
                    break;
                case "datetime":
                    DateTime parsedDateTime;
                    if (!DateTime.TryParse(dataDictionaryValues[0].ToString(), out parsedDateTime))
                        hasTypeError = true;
                    break;
                case "email":
                    try {
                        var validEmailAddress = new System.Net.Mail.MailAddress(dataDictionaryValues[0].ToString());
                    }
                    catch {
                        hasTypeError = true;
                    }
                    break;
                case "month":
                    try {
                        DateTime parsedMonth = DateTime.ParseExact(dataDictionaryValues[0].ToString(), "yyyy-MM", CultureInfo.InvariantCulture);
                    }
                    catch (FormatException) {
                        hasTypeError = true;
                    }
                    break;
                case "int":
                    isNumber = true;
                    // Valid integer number values are C# ints
                    int parsedInt;
                    if (!int.TryParse(dataDictionaryValues[0].ToString(), out parsedInt))
                        hasTypeError = true;
                    parsedNumber = parsedInt;
                    break;
                case "float":
                    isNumber = true;
                    // Valid floatiog point number values are C# floats
                    try
                    {
                        // CultureInfo.InvariantCulture is necessary since server might have different culture
                        parsedNumber = float.Parse(dataDictionaryValues[0].ToString(), CultureInfo.InvariantCulture);
                    }
                    catch (FormatException) {
                        hasTypeError = true;
                    }
                    break;
                case "year":
                    isNumber = true;
                    // Valid year values are between -9999 and 9999
                    int parsedYear;
                    if (!int.TryParse(dataDictionaryValues[0].ToString(), out parsedYear) || parsedYear < -9999 || parsedYear > 9999)
                        hasTypeError = true;
                    parsedNumber = parsedYear;
                    break;
                case "phone":
                    if (!dataDictionaryValues[0].ToString().All(c => "0123456789!().-,".Contains(c)))
                        hasTypeError = true;
                    break;
                case "string":
                    isText = true;
                    // String can contain anything, no type validation is needed.
                    break;
                case "time":
                    try {
                        DateTime parsedMonth = DateTime.ParseExact(dataDictionaryValues[0].ToString(), "hh:mm", CultureInfo.InvariantCulture);
                    }
                    catch (FormatException) {
                        hasTypeError = true;
                    }
                    break;
                case "url":
                    Uri outUri;
                    if (!Uri.TryCreate(dataDictionaryValues[0].ToString(), UriKind.RelativeOrAbsolute, out outUri))
                        hasTypeError = true;
                    break;
                case "username":
                    isText = true;
                    // String can contain anything, no type validation is needed.
                    break;
                case "bool":
                    // Valid boolean values are 0 or 1
                    int parsedBool;
                    if (!int.TryParse(dataDictionaryValues[0].ToString(), out parsedBool) || parsedBool != 0 || parsedBool != 1)
                        hasTypeError = true;
                    break;
                case "text":
                    isText = true;
                    // String can contain anything, no type validation is needed.
                    break;
                default:
                    isReference = true;
                    // References must contain only valid ids to the referenced dataset
                    var validIds = validReferencesIdsDictionary.FirstOrDefault(k => k.Key == attributeDescriptor.Type);
                    if (validIds.Equals(new KeyValuePair<string, List<long>>()))
                    {
                        // Key error
                        var message = new Message(MessageTypeEnum.Error, 
                                                   6015, 
                                                   new List<string>(){ attributeDescriptor.Type, attributeDescriptor.Name }, 
                                                   attributeDescriptor.Name);
                        Logger.LogMessageToConsole(message);
                        messages.Add(message);
                        return messages;
                    }
                    foreach (var item in dataDictionaryValues)
                    {
                        long parsedReference;
                        // If reference is not a valid number
                        if (!long.TryParse(item.ToString(), out parsedReference))
                        {
                            var message = new Message(MessageTypeEnum.Error, 
                                                   6016, 
                                                   new List<string>(){ attributeDescriptor.Name, item.ToString() }, 
                                                   attributeDescriptor.Name);
                            messages.Add(message);
                        }
                        // If reference id is not a valid id from database
                        else if (!validIds.Value.Contains(parsedReference))
                        {
                            var message = new Message(MessageTypeEnum.Error, 
                                                   6017, 
                                                   new List<string>(){ parsedReference.ToString(), attributeDescriptor.Name }, 
                                                   attributeDescriptor.Name);
                            messages.Add(message);
                        } 
                    }
                    break;
            }
            // If attribute type is not reference, check that dataDictionaryValues contains only one item
            if (!isReference)
            {
                if (dataDictionaryValues.Count != 1)
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6003, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          dataDictionaryValues.Count.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
                    // If tehere are more items, there is no need for other validations
                    return messages;
                }
            }
            // If validations found any type errors
            if (hasTypeError)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                        6004, 
                                        new List<string>(){ attributeDescriptor.Name,
                                                            attributeDescriptor.Type,
                                                            dataDictionaryValues[0].ToString()}, 
                                        attributeDescriptor.Name)
                            );
                // If value is not correct, there is no need for other validations
                return messages;
            }

            // Unique
            // not implemented 

            // Min and max
            if (isNumber)
            {
                if (attributeDescriptor.Min != null && parsedNumber < attributeDescriptor.Min)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6005, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          attributeDescriptor.Min.ToString(),
                                                                          parsedNumber.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
                else if (attributeDescriptor.Max != null && parsedNumber > attributeDescriptor.Max)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6006, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          attributeDescriptor.Max.ToString(),
                                                                          parsedNumber.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
            }
            else if (isText)
            {
                if (attributeDescriptor.Min != null && dataDictionaryValues[0].ToString().Length < attributeDescriptor.Min)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6007, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          attributeDescriptor.Min.ToString(),
                                                                          dataDictionaryValues[0].ToString(),
                                                                          dataDictionaryValues[0].ToString().Length.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
                else if (attributeDescriptor.Max != null && dataDictionaryValues[0].ToString().Length > attributeDescriptor.Max)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6008, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          attributeDescriptor.Max.ToString(),
                                                                          dataDictionaryValues[0].ToString(),
                                                                          dataDictionaryValues[0].ToString().Length.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
            }
            else if (isReference)
            {
                if (attributeDescriptor.Min != null && dataDictionaryValues.Count < attributeDescriptor.Min)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6009, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          attributeDescriptor.Min.ToString(),
                                                                          dataDictionaryValues.Count.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
                else if (attributeDescriptor.Max != null && dataDictionaryValues.Count > attributeDescriptor.Max)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6010, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          attributeDescriptor.Max.ToString(),
                                                                          dataDictionaryValues.Count.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
            }

            return messages;
        }
    }
}