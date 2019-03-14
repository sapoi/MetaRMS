using System;
using System.Collections.Generic;
using System.Linq;
using RazorWebApp.Repositories;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;

namespace RazorWebApp.Cache
{
    public class ReferenceCache
    {
        Dictionary<string, Dictionary<long, string>> referenceCache;
        DatabaseContext context;
        ApplicationModel applicationModel;
        public ReferenceCache(DatabaseContext context, ApplicationModel applicationModel)
        {
            this.referenceCache = new Dictionary<string, Dictionary<long, string>>();
            this.context = context;
            this.applicationModel = applicationModel;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public string GetTextForReference(string type, long id, int level = 0)
        {
            // If text is already in cache, just return it
            if (referenceCache.ContainsKey(type) && referenceCache[type].ContainsKey(id))
                return referenceCache[type][id];
            // Otherwise load it into cache and return it
            string value = "";
            bool found = false;
            if (type == applicationModel.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name)
            {
                UserRepository userRepository = new UserRepository(context);
                UserModel userModel = userRepository.GetById(id);
                found = true;
                value = userModel.GetUsername();
            }
            else
            {
                DataRepository dataRepository = new DataRepository(context);
                DataModel dataModel = dataRepository.GetById(id);
                DatasetDescriptor datasetDescriptor = applicationModel.ApplicationDescriptor.Datasets.Where(d => d.Name == type).FirstOrDefault();
                if (datasetDescriptor == null)
                    throw new Exception($"[ERROR]: Dataset {type} not in application {applicationModel.LoginApplicationName} with id {applicationModel.Id}.\n");
                string text = "";
                List<AttributeDescriptor> attributeDescriptors = new List<AttributeDescriptor>();
                for (int i = 0; i < Math.Min(3, datasetDescriptor.Attributes.Count); i++)
                    attributeDescriptors.Add(datasetDescriptor.Attributes[i]);
                var lastAttributeDescriptor = attributeDescriptors.Last();
                foreach (var attributeDescriptor in attributeDescriptors)
                {
                    List<object> data = dataModel.DataDictionary[attributeDescriptor.Name];
                    for (int j = 0; j < Math.Min(2, data.Count); j++)
                    {
                        // pokud je atribut reference, dohledej jeho hodnotu
                        long dataId;
                        bool isReference = !AttributeType.Types.Contains(attributeDescriptor.Type);
                        if (isReference && level <= 3 && long.TryParse(data[j].ToString(), out dataId))
                            text += "(" + GetTextForReference(attributeDescriptor.Type, dataId, level++) + ")";
                        else
                        {
                            if (!isReference)
                                found = true;
                            text += data[j].ToString();
                        }
                        if (!(j == Math.Min(2, data.Count)-1))
                            text += ", ";
                    }
                    if (!attributeDescriptor.Equals(lastAttributeDescriptor))
                    {
                        text += " | ";
                    }
                }
                value = text;
            }
            if (found)
            {
                if (!referenceCache.ContainsKey(type))
                    referenceCache.Add(type, new Dictionary<long, string>() { { id, value } });
                else
                    referenceCache[type].Add(id, value);
            }
            return value;
        }
    }
}