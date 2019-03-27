using System;
using System.Collections.Generic;
using System.Linq;
using RazorWebApp.Repositories;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using System.Text;

namespace RazorWebApp.Cache
{
    public class ReferenceCache
    {
        Dictionary<string, Dictionary<long, string>> referenceCache;
        DatabaseContext context;
        ApplicationModel applicationModel;
        UserRepository userRepository;
        DataRepository dataRepository;
        public ReferenceCache(DatabaseContext context, ApplicationModel applicationModel)
        {
            this.referenceCache = new Dictionary<string, Dictionary<long, string>>();
            this.context = context;
            this.applicationModel = applicationModel;
            this.userRepository = new UserRepository(context);
            this.dataRepository = new DataRepository(context);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Type of the reference</param>
        /// <param name="id">Id to get the text representation for</param>
        /// <param name="level"></param>
        /// <returns></returns>
        public string GetTextForReference(string type, long id, int level = 0)
        {
            // If text is already in cache, just return it
            if (referenceCache.ContainsKey(type) && referenceCache[type].ContainsKey(id))
               return referenceCache[type][id];
            // Otherwise load it into cache and return it
            string value = "";
            // If reference is of type system user
            if (type == applicationModel.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name)
            {
                UserModel userModel = userRepository.GetById(id);
                value = userModel.GetUsername();
                addToCache(type, id, value);
            }
            // If reference is of type user defined dataset
            else
            {
                DataModel dataModel = dataRepository.GetById(id);
                DatasetDescriptor datasetDescriptor = applicationModel.ApplicationDescriptor.Datasets.Where(d => d.Name == type).FirstOrDefault();
                if (datasetDescriptor == null)
                    throw new Exception($"[ERROR]: Dataset {type} not in application {applicationModel.LoginApplicationName} with id {applicationModel.Id}.\n");
                var sb = new StringBuilder("");
                // Get text representation for first at most 3 attributes of dataModel
                foreach (var attributeDescriptor in datasetDescriptor.Attributes.Take(3))
                {
                    // For basic types get the value and for reference types get first 3  references and get their text representation
                    foreach (var dataDictionaryValue in dataModel.DataDictionary[attributeDescriptor.Name].Take(3))
                    {
                        // If dataDictionaryValue is reference, get its representation
                        bool isReference = !AttributeType.Types.Contains(attributeDescriptor.Type);
                        if (isReference)
                        {
                            long dataId;
                            if (level > 4)
                                sb.Append("...");
                            else if (long.TryParse(dataDictionaryValue.ToString(), out dataId))
                                sb.Append("(" + GetTextForReference(attributeDescriptor.Type, dataId, ++level) + ")");
                            else
                            {
                                // Error - reference could not be parsed
                                sb.Append("!!!");
                            }
                        }
                        // If dataDictionaryValue is not reference, get its value
                        else
                        {
                            sb.Append(dataDictionaryValue);
                        }
                        if (!attributeDescriptor.Equals(dataModel.DataDictionary[attributeDescriptor.Name].Take(3).Last()))
                            sb.Append(", ");
                    }
                    if (!attributeDescriptor.Equals(datasetDescriptor.Attributes.Take(3).Last()))
                    {
                        sb.Append(" | ");
                    }
                }
                value = sb.ToString();
                addToCache(type, id, value);
            }
            return value;
        }
        void addToCache(string type, long id, string value)
        {
            if (!referenceCache.ContainsKey(type))
                referenceCache.Add(type, new Dictionary<long, string>() { { id, value } });
            else if (!referenceCache[type].ContainsKey(id))
                referenceCache[type].Add(id, value);
        }
    }
}