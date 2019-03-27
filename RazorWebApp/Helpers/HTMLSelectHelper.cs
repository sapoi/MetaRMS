using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;
using SharedLibrary.Services;
using System.Net.Http;
using SharedLibrary.Structures;
using System.Linq;
using SharedLibrary.Models;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using System.Text;

namespace RazorWebApp.Helpers
{
    /// <summary>
    /// This helper is used by Razor pages for loading additional values for selects.
    /// </summary>
    public class HTMLSelectHelper
    {
        /// <summary>
        /// This method is used to load all available RightsModels for an application from the server.
        /// These models are then converted to the list of SelectListItem that can be used in HTML
        /// selects to pick a rights for user.
        /// </summary>
        /// <param name="rightsService">Rights service to conntect to the server</param>
        /// <param name="token">JWT token to authenticate at the server</param>
        /// <returns>List of SelectListItem</returns>
        public async Task<List<SelectListItem>> FillUserRightsData(IRightsService rightsService, JWTToken token)
        {
            var response = await rightsService.GetAll(token);
            // If server did not return the rights successfully
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogToConsole($"For user with token {token.Value} rights to select did not load successfully from the server.");
                return new List<SelectListItem>();
            };
            // Deserialize response
            List<RightsModel> data = JsonConvert.DeserializeObject<List<RightsModel>>(await response.Content.ReadAsStringAsync());
            // Transform it to the select list
            return data.Select(x => 
                                new SelectListItem
                                {
                                    Value = x.Id.ToString(),
                                    Text = x.Name
                                })
                       .ToList();
        }
        /// <summary>
        /// This method is used to load all available DataModels and UserModels for an application
        /// from the server. These models are converted to a list of SelectListItem that can be used
        /// in HTML selects.
        /// </summary>
        /// <param name="applicationDescriptor">Descriptor of application to load the data from</param>
        /// <param name="attributes">Attributes to load the values for</param>
        /// <param name="userService">User service to conntect to the server</param>
        /// <param name="dataService">Data service to conntect to the server</param>
        /// <param name="token">JWT token to authenticate at the server</param>
        /// <returns>Dictionary with attribute types as key and list of the available values as a value.</returns>
        public async Task<Dictionary<string, List<SelectListItem>>> FillSelectData(ApplicationDescriptor applicationDescriptor, 
            List<AttributeDescriptor> attributes, IUserService userService, IDataService dataService, JWTToken token)
        {
            var selectData = new Dictionary<string, List<SelectListItem>>();
            foreach (var attribute in attributes)
            {
                // If attribute is reference type
                if (!AttributeType.Types.Contains(attribute.Type))
                    // And is not already in the selectData
                    if (!selectData.ContainsKey(attribute.Type))
                    {
                        // Get available values from server
                        HttpResponseMessage response;
                        List<AttributeDescriptor> shownAttributes = new List<AttributeDescriptor>();
                        selectData.Add(attribute.Type, new List<SelectListItem>());
                        // If attribute type reference is system user dataset
                        if (attribute.Type == applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name)
                        {
                            response = await userService.GetAll(token);
                            // If response from the server was not successfull, add empty list for attribute type key
                            if (!response.IsSuccessStatusCode)
                            {
                                Logger.LogToConsole($"DataLoadingHepler request to system users dataset {attribute.Type} did not return any results in application {applicationDescriptor.LoginApplicationName}.");
                                selectData[attribute.Type] = new List<SelectListItem>();
                                continue;
                            }
                            List<UserModel> userModelList = JsonConvert.DeserializeObject<List<UserModel>>(await response.Content.ReadAsStringAsync());
                            // Add UserModels from response to the attribute type key
                            foreach (var item in userModelList)
                                selectData[attribute.Type].Add(new SelectListItem { Value = item.Id.ToString(), Text = item.GetUsername() });
                        }
                        // If attribute type reference is user defined dataset
                        else
                        {
                            var sourceDataset = applicationDescriptor.Datasets.FirstOrDefault(d => d.Name == attribute.Type);
                            // If source dataset was not found in the application descriptor
                            if (sourceDataset == null)
                            {
                                Logger.LogToConsole($"DataLoadingHepler source name {attribute.Type} not found in user defined descriptors in application {applicationDescriptor.LoginApplicationName}.");
                                selectData[attribute.Type] = new List<SelectListItem>();
                                continue;
                            }
                            response = await dataService.GetAll(sourceDataset.Id, token);
                            // If response from the server was not successfull, add empty list for attribute type key
                            if (!response.IsSuccessStatusCode)
                            {
                                Logger.LogToConsole($"DataLoadingHepler request to user defined dataset {attribute.Type} did not return any results in application {applicationDescriptor.LoginApplicationName}.");
                                selectData[attribute.Type] = new List<SelectListItem>();
                                continue;
                            }
                            List<DataModel> dataModelList = JsonConvert.DeserializeObject<List<DataModel>>(await response.Content.ReadAsStringAsync());
                            // At most first 3 attribues of dataset are shown in select 
                            shownAttributes.Add(sourceDataset.Attributes[0]);
                            if (sourceDataset.Attributes.Count > 1)
                                shownAttributes.Add(sourceDataset.Attributes[1]);
                            if (sourceDataset.Attributes.Count > 2)
                                shownAttributes.Add(sourceDataset.Attributes[2]);
                            // Get text representation for each model
                            foreach (var item in dataModelList)
                            {
                                string text = getTextForSelectItem(shownAttributes, item.DataDictionary);
                                selectData[attribute.Type].Add(new SelectListItem { Value = item.Id.ToString(), Text = text });
                            }
                        }
                    }
            }
            return selectData;
        }
        /// <summary>
        /// This method returns string representation of a data dictionary based on its attributes.
        /// </summary>
        /// <param name="shownAttributes">List of attributes that will be displayed at the text</param>
        /// <param name="dataDictionary">Dictionary to get the text values from</param>
        /// <returns>String value from given parametres</returns>
        string getTextForSelectItem(List<AttributeDescriptor> shownAttributes, Dictionary<string, List<object>> dataDictionary)
        {
            var sb = new StringBuilder("");
            foreach (var attribute in shownAttributes)
            {
                // If attribute is of a basic type, return just its value
                if (AttributeType.Types.Contains(attribute.Type))
                {
                    sb.Append(dataDictionary[attribute.Name].FirstOrDefault());
                }
                // If the attribute is of reference type get first at most 3 values and build them into one string
                else 
                {
                    if (dataDictionary[attribute.Name].Count > 0)
                    {
                        sb.Append("(");
                        sb.Append(JsonConvert.DeserializeObject<Tuple<string, string>>(dataDictionary[attribute.Name][0].ToString()).Item2);
                        if (dataDictionary[attribute.Name].Count > 1)
                        {
                            sb.Append(", ");
                            sb.Append(JsonConvert.DeserializeObject<Tuple<string, string>>(dataDictionary[attribute.Name][1].ToString()).Item2);
                            if (dataDictionary[attribute.Name].Count > 2)
                            {
                                sb.Append(", ");
                                sb.Append(JsonConvert.DeserializeObject<Tuple<string, string>>(dataDictionary[attribute.Name][2].ToString()).Item2);
                            }
                        }
                        sb.Append(")");
                    }
                }
                if (attribute != shownAttributes.Last())
                    sb.Append(" | ");
            }
            return sb.ToString();
        }
    }
}