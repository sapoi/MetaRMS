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

namespace SharedLibrary.Helpers
{
    /// <summary>
    /// The ValidationsHelper class contains helper methods for input validations by application descriptor.
    /// </summary>
    public class ValidationsHelper
    {
        /// <summary>
        /// Returns true of password fulfills safety requirements defined by the regular expression
        /// from http://html5pattern.com/Passwords (at least 8 characters long and contain at least 
        /// 1 upper and 1 lower-case letter and one number or special character)
        /// </summary>
        /// <param name="password">String password to be validated</param>
        /// <returns>True if password is safe, otherwise false</returns>
        public bool IsPasswordSafer(string password)
        {
            return Regex.IsMatch(password, @"(?=^.{8,}$)((?=.*\d)|(?=.*\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$");
        }
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
        public List<Message> ValidateRights(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rightsData)
        {
            // Check that all oligatory rights are filled and are in a correct format
            var messages = validateRightsData(applicationDescriptor, rightsData);
            // Only if the rights are correct, check the logical validity
            if (messages == null || messages.Count == 0)
                messages.AddRange(validateRightsLogic(applicationDescriptor, rightsData));
            return messages;
        }
        List<Message> validateRightsData(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rightsData)
        {
            var messages = new List<Message>();
            // Check user defined datasets
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
                var rightsForDataset = rightsData.FirstOrDefault(r => r.Key == datasetDescriptor.Id);
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
        List<Message> validateRightsLogic(ApplicationDescriptor applicationDescriptor, Dictionary<long, RightsEnum> rightsData)
        {
            var messages = new List<Message>();
            // Check user defined datasets
            var datasetsToCheck = new List<DatasetDescriptor>();
            datasetsToCheck.AddRange(applicationDescriptor.Datasets);
            // And users dataset
            datasetsToCheck.Add(applicationDescriptor.SystemDatasets.UsersDatasetDescriptor);
            // Dictionary containing <dataset id, dataset name>
            var idNameDictionary = datasetsToCheck.ToDictionary(d => d.Name, d => d.Id);
            // Check that for each dataset with at least read right, all datasets referenced in it have at least read rights
            foreach (var datasetDescriptor in datasetsToCheck)
            {
                if (rightsData[datasetDescriptor.Id] >= RightsEnum.R)
                {
                    foreach (var attribute in datasetDescriptor.Attributes)
                    {
                        // If attribute type is reference
                        if (!AttributeType.Types.Contains(attribute.Type))
                        {
                            // And rights for the reference are less than read rights
                            if (rightsData[idNameDictionary[attribute.Type]] < RightsEnum.R)
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
        public List<Message> ValidateDataByApplicationDescriptor(DatasetDescriptor datasetDescriptor, Dictionary<string, List<object>> data, Dictionary<string, List<long>> validReferencesIdsDictionary)
        {
            var messages = new List<Message>();
            foreach (var item in data)
            {
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
                var attributeMessages = validateOneAttribute(attributeDescriptor, item.Value, validReferencesIdsDictionary);
                if (attributeMessages != null)
                    messages.AddRange(attributeMessages);
            }
            return messages;
        }
        List<Message> validateOneAttribute(AttributeDescriptor attributeDescriptor, List<object> inputData, Dictionary<string, List<long>> validReferencesIdsDictionary)
        {
            var messages = new List<Message>();

            // Required
            // if attribute is required and there are no input data, or they are empty or the first element is empty return error message
            if (attributeDescriptor.Required == true && (inputData == null || inputData.Count == 0 || inputData[0].ToString() == ""))
            {
                var message = new Message(MessageTypeEnum.Error, 
                                                   6002, 
                                                   new List<string>(){ attributeDescriptor.Name }, 
                                                   attributeDescriptor.Name);
                messages.Add(message);
                return messages;
            }
            // if the attribute is not required and is empty, no validation is needed, so return empty messages
            if (inputData == null || inputData.Count == 0 || inputData[0].ToString() == "")
                return messages;

            // general data types later used for min and max validation
            bool isNumber = false;
            double parsedNumber = 0;
            bool isText = false;
            bool isReference = false;

            // Type
            bool hasTypeError = false;
            switch (attributeDescriptor.Type)
            {
                case "color":
                    if (!Regex.Match(inputData[0].ToString(), "^#(?:[0-9a-fA-F]{3}){1,2}$").Success)
                        hasTypeError = true;
                    break;
                case "date":
                    try {
                        DateTime parsedDate = DateTime.ParseExact(inputData[0].ToString(), "d", CultureInfo.InvariantCulture);
                    }
                    catch (FormatException) {
                        hasTypeError = true;
                    }
                    break;
                case "datetime":
                    DateTime parsedDateTime;
                    if (!DateTime.TryParse(inputData[0].ToString(), out parsedDateTime))
                        hasTypeError = true;
                    break;
                case "email":
                    try {
                        var validEmailAddress = new System.Net.Mail.MailAddress(inputData[0].ToString());
                    }
                    catch {
                        hasTypeError = true;
                    }
                    break;
                case "month":
                    try {
                        DateTime parsedMonth = DateTime.ParseExact(inputData[0].ToString(), "yyyy-MM", CultureInfo.InvariantCulture);
                    }
                    catch (FormatException) {
                        hasTypeError = true;
                    }
                    break;
                case "int":
                    isNumber = true;
                    // Valid year values are C# ints
                    int parsedInt;
                    if (!int.TryParse(inputData[0].ToString(), out parsedInt))
                        hasTypeError = true;
                    parsedNumber = parsedInt;
                    break;
                case "float":
                    isNumber = true;
                    // Valid year values are C# floats
                    float parsedFloat;
                    if (!float.TryParse(inputData[0].ToString(), out parsedFloat))
                        hasTypeError = true;
                    parsedNumber = parsedFloat;
                    break;
                case "year":
                    isNumber = true;
                    // Valid year values are between -9999 and 9999
                    int parsedYear;
                    if (!int.TryParse(inputData[0].ToString(), out parsedYear) || parsedYear < -9999 || parsedYear > 9999)
                        hasTypeError = true;
                    parsedNumber = parsedYear;
                    break;
                case "tel":
                    if (!inputData[0].ToString().All(c => "0123456789!().-,".Contains(c)))
                        hasTypeError = true;
                    break;
                case "string":
                    isText = true;
                    // String can contain anything, no type validation is needed.
                    break;
                case "time":
                    try {
                        DateTime parsedMonth = DateTime.ParseExact(inputData[0].ToString(), "hh:mm", CultureInfo.InvariantCulture);
                    }
                    catch (FormatException) {
                        hasTypeError = true;
                    }
                    break;
                case "url":
                    Uri outUri;
                    if (!Uri.TryCreate(inputData[0].ToString(), UriKind.RelativeOrAbsolute, out outUri))
                        hasTypeError = true;
                    break;
                case "username":
                    isText = true;
                    // String can contain anything, no type validation is needed.
                    break;
                case "bool":
                    // Valid boolean values are 0 or 1
                    int parsedBool;
                    if (!int.TryParse(inputData[0].ToString(), out parsedBool) || parsedBool != 0 || parsedBool != 1)
                        hasTypeError = true;
                    break;
                case "text":
                    isText = true;
                    // String can contain anything, no type validation is needed.
                    break;
                default:
                    isReference = true;
                    // References must contain only valid ids to the referenced dataset
                    var validIds = validReferencesIdsDictionary.FirstOrDefault(k => k.Key == attributeDescriptor.Name);
                    if (validIds.Equals(new KeyValuePair<string, List<long>>()))
                    {
                        // Key error
                    }
                    foreach (var item in inputData)
                    {
                        long parsedReference;
                        // If reference is not a valid number
                        if (!long.TryParse(item.ToString(), out parsedReference))
                        {

                        }
                        // If reference id is not a valid id from database
                        else if (!validIds.Value.Contains(parsedReference))
                        {

                        } 
                    }
                    break;
            }
            // If attribute type is not reference, check that inputData contains only one item
            if (!isReference)
            {
                if (inputData.Count != 1)
                {
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6003, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          inputData.Count.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
                    // If tehere are more items, there is no need for other validations
                    return messages;
                }
            }
            // If validations found any type errors
            if (hasTypeError)
            {
                messages.Add(createBasicValidationMessage(attributeDescriptor.Name,
                                                          attributeDescriptor.Type,
                                                          inputData[0].ToString()
                                                         )
                            );
                // If value is not correct, there is no need for other validations
                return messages;
            }

            // Unique
            // not implemented yet

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
                if (attributeDescriptor.Min != null && inputData[0].ToString().Length < attributeDescriptor.Min)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6007, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          attributeDescriptor.Min.ToString(),
                                                                          inputData[0].ToString(),
                                                                          inputData[0].ToString().Length.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
                else if (attributeDescriptor.Max != null && inputData[0].ToString().Length > attributeDescriptor.Max)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6008, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          attributeDescriptor.Max.ToString(),
                                                                          inputData[0].ToString(),
                                                                          inputData[0].ToString().Length.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
            }
            else if (isReference)
            {
                if (attributeDescriptor.Min != null && inputData.Count < attributeDescriptor.Min)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6009, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          attributeDescriptor.Min.ToString(),
                                                                          inputData.Count.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
                else if (attributeDescriptor.Max != null && inputData.Count > attributeDescriptor.Max)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                                      6010, 
                                                      new List<string>(){ attributeDescriptor.Name,
                                                                          attributeDescriptor.Max.ToString(),
                                                                          inputData.Count.ToString() }, 
                                                      attributeDescriptor.Name)
                                );
            }
            return messages;
        }
        Message createBasicValidationMessage(string attributeName, string attributeType, string data)
        {
            return new Message(MessageTypeEnum.Error, 
                                        6004, 
                                        new List<string>(){ attributeName,
                                                            attributeType,
                                                            data}, 
                                        attributeName);
        }
    }
}