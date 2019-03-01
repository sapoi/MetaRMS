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

namespace RazorWebApp.Helpers
{
    public class DataLoadingHelper
    {
        // frontend
        public async Task<IEnumerable<SelectListItem>> FillUserRightsData(IRightsService rightsService, JWTToken.AccessToken token)
        {
            var response = await rightsService.GetAll(token.Value);
            //TODO kontrolovat chyby v response
            string stringResponse = await response.Content.ReadAsStringAsync();
            List<RightsModel> data = JsonConvert.DeserializeObject<List<RightsModel>>(stringResponse);

            return data.Select(x => 
                                            new SelectListItem
                                            {
                                                Value = x.Id.ToString(),
                                                Text = x.Name
                                            });
        }
        public async Task<Dictionary<string, List<SelectListItem>>> FillSelectData(ApplicationDescriptor applicationDescriptor, List<AttributeDescriptor> attributes, 
            IUserService userService, IDataService dataService, JWTToken.AccessToken token)
        {
            var selectData = new Dictionary<string, List<SelectListItem>>();
            foreach (var attribute in attributes)
            {
                if (!AttributeType.Types.Contains(attribute.Type))
                    if (!selectData.ContainsKey(attribute.Type))
                    {
                        // getting real data
                        // if type is user
                        HttpResponseMessage response;
                        List<AttributeDescriptor> shownAttributes = new List<AttributeDescriptor>();
                        
                        if (attribute.Type == applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name)
                        {
                            response = await userService.GetAll(token.Value);
                            // kontrolovat response
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
                            var sourceDataset = applicationDescriptor.Datasets.FirstOrDefault(d => d.Name == attribute.Type);
                            if (sourceDataset == null)
                            {
                                //TODO server error
                                Logger.LogToConsole("DataLoadingHepler source name not found in user defined descriptors.");
                                break;
                            }
                            response = await dataService.GetAll(sourceDataset.Id, token.Value);
                            // kontrolovat response
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
            return text;
        }
    }
}