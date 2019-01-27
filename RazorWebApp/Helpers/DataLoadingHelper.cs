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

namespace RazorWebApp.Helpers
{
    public class DataLoadingHelper
    {
        public async Task<Dictionary<string, List<SelectListItem>>> FillSelectData(ApplicationDescriptor applicationDescriptor, List<AttributeDescriptor> attributes, 
            IUserService userService, IDataService dataService, JWTToken.AccessToken token)
        {
            var selectData = new Dictionary<string, List<SelectListItem>>();
            foreach (var attribute in attributes)
            {
                if (attribute.Type != "color" && attribute.Type != "date" && attribute.Type != "datetime" && 
                    attribute.Type != "email" && attribute.Type != "month" && attribute.Type != "int" && 
                    attribute.Type != "float" && attribute.Type != "year" && attribute.Type != "tel" && 
                    attribute.Type != "string" && attribute.Type != "time" && attribute.Type != "url" &&
                    attribute.Type != "bool" && attribute.Type != "text" &&
                    attribute.Type != "username" /* && attribute.Type != "password" */)
                    if (!selectData.ContainsKey(attribute.Type))
                    {
                        // getting real data
                        // if type is user
                        HttpResponseMessage response;
                        List<AttributeDescriptor> shownAttributes = new List<AttributeDescriptor>();
                        
                        if (attribute.Type == applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name)
                        {
                            response = await userService.GetAll(applicationDescriptor.LoginAppName, token.Value);
                            string stringResponse = await response.Content.ReadAsStringAsync();
                            List<UserModel> data = JsonConvert.DeserializeObject<List<UserModel>>(stringResponse);
                            shownAttributes.Add(applicationDescriptor.GetUsernameAttribute());

                            selectData.Add(attribute.Type, new List<SelectListItem>());
                            foreach (var item in data)
                            {
                                selectData[attribute.Type].Add(new SelectListItem { Value = item.Id.ToString(), Text = item.GetUsername() });
                            }
                        }
                        // if type is any otherreference data
                        else
                        {
                            response = await dataService.GetAll(applicationDescriptor.LoginAppName, attribute.Type, token.Value);
                            string stringResponse = await response.Content.ReadAsStringAsync();
                            List<DataModel> data = JsonConvert.DeserializeObject<List<DataModel>>(stringResponse);
                            var st = applicationDescriptor.Datasets.Where(d => d.Name == attribute.Type).First();
                            // at most first 3 attribues of dataset are shown in select 
                            shownAttributes.Add(st.Attributes[0]);
                            if (st.Attributes.Count > 1)
                                shownAttributes.Add(st.Attributes[1]);
                            if (st.Attributes.Count > 2)
                                shownAttributes.Add(st.Attributes[2]);

                            selectData.Add(attribute.Type, new List<SelectListItem>());
                            foreach (var item in data)
                            {
                                string text = getTextForSelectItem(shownAttributes, item.DataDictionary);
                                selectData[attribute.Type].Add(new SelectListItem { Value = item.Id.ToString(), Text = text });
                            }
                        }
                        //TODO kontrolovat chyby v response
                        // string stringResponse = await response.Content.ReadAsStringAsync();
                        // var data = JsonConvert.DeserializeObject<List<Dictionary<String, Object>>>(stringResponse);
                        // selectData.Add(attribute.Type, new List<SelectListItem>());
                        // foreach (var item in data)
                        // {
                        //     string value = JsonConvert.DeserializeObject<List<string>>(item["DBId"].ToString()).First();
                        //     string text = getTextForSelectItem(shownAttributes, item);
                        //     selectData[attribute.Type].Add(new SelectListItem { Value = value, Text = text });
                        // }
                        // selectData[attribute.Type] = data.Select(x => new SelectListItem { Value =  JsonConvert.DeserializeObject<List<string>>
                        //                                                                                 (x["DBId"].ToString()).First(), 
                        //                                                                     Text =  JsonConvert.DeserializeObject<List<string>>
                        //                                                                                 (x[shownAttributes[0].Name
                        //                                                                                     ].ToString()).First() 
                        //                                                                                     +
                        //                                                                             JsonConvert.DeserializeObject<List<string>>
                        //                                                                                 (x[shownAttributes[1].Name
                        //                                                                                     ].ToString()).First()
                        //                                                                     } 
                        //                                         )
                        //                                     .ToList();
                    }
            }
            return selectData;
        }
        string getTextForSelectItem(List<AttributeDescriptor> shownAttributes, Dictionary<string, List<object>> item)
        {
            string text = "";
            foreach (var attribute in shownAttributes)
            {
                // basic type - take just the value
                if (AttributeType.Types.Contains(attribute.Type))
                {
                    text += item[attribute.Name].FirstOrDefault();
                }
                // reference type - value needs to be deserialized
                else 
                {
                    if (item[attribute.Name].Count > 0)
                    {
                        text += "(";
                        text += JsonConvert.DeserializeObject<Tuple<string, string>>(item[attribute.Name][0].ToString()).Item2;
                        if (item[attribute.Name].Count > 1)
                        {
                            text += ", ";
                            text += JsonConvert.DeserializeObject<Tuple<string, string>>(item[attribute.Name][1].ToString()).Item2;
                            if (item[attribute.Name].Count > 2)
                            {
                                text += ", ";
                                text += JsonConvert.DeserializeObject<Tuple<string, string>>(item[attribute.Name][2].ToString()).Item2;
                            }
                        }
                        text += ")";
                    }
                }
                if (attribute != shownAttributes.Last())
                    text += " | ";
            }
            // if (text.Length >= 3)
            //     text = text.Substring(0, text.Length - 3);
            return text;
        }
    }
}