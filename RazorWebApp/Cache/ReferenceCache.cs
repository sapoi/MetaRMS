using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RazorWebApp.Repositories;
using Server;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using SharedLibrary.Services;
using static SharedLibrary.Structures.JWTToken;

namespace RazorWebApp.Cache
{
    public class ReferenceCache
    {
        Dictionary<string, Dictionary<long, string>> _referenceCache;
        DatabaseContext _context;
        ApplicationModel _application;
        public ReferenceCache(DatabaseContext context, ApplicationModel application)
        {
            _referenceCache = new Dictionary<string, Dictionary<long, string>>();
            _context = context;
            _application = application;
        }
        public string getTextForReference(string type, long id, int level = 0)
        {
            // if text is already in cache, just return it
            if (_referenceCache.ContainsKey(type) && _referenceCache[type].ContainsKey(id))
                return _referenceCache[type][id];
            // otherwise load it into cache and return it
            string value = "";
            bool found = false;
            if (type == _application.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name)
            {
                UserRepository userRepository = new UserRepository(_context);
                UserModel userModel = userRepository.GetById(id);
                found = true;
                value = userModel.GetUsername();
            }
            else
            {
                DataRepository dataRepository = new DataRepository(_context);
                DataModel dataModel = dataRepository.GetById(id);
                DatasetDescriptor datasetDescriptor = _application.ApplicationDescriptor.Datasets.Where(d => d.Name == type).FirstOrDefault();
                if (datasetDescriptor == null)
                    throw new Exception($"[ERROR]: Dataset {type} not in application {_application.LoginApplicationName} with id {_application.Id}.\n");
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
                            text += "(" + getTextForReference(attributeDescriptor.Type, dataId, level++) + ")";
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
                        // if (level == 0)
                        //     text += " - ";
                        // else
                        text += " | ";
                    }
                }
                value = text;
            }
            if (found)
            {
                if (!_referenceCache.ContainsKey(type))
                    _referenceCache.Add(type, new Dictionary<long, string>() { { id, value } });
                else
                    _referenceCache[type].Add(id, value);
            }
            return value;
        }
    }
}