using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RazorWebApp;
using RazorWebApp.Cache;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;

namespace SharedLibrary.Helpers
{
    // backend
    /// <summary>
    /// 
    /// </summary>
    public class DataHelper
    {
        List<(string Name, string Type)> referenceIndexTypeTuple;
        /// <summary>
        /// 
        /// </summary>
        ApplicationModel applicationModel;
        /// <summary>
        /// Cache for reading and storing references
        /// </summary>
        ReferenceCache referenceCache;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseContext"></param>
        /// <param name="applicationModel"></param>
        /// <param name="datasetId"></param>
        public DataHelper(DatabaseContext databaseContext, ApplicationModel applicationModel, long datasetId)
        {
            this.applicationModel = applicationModel;
            this.referenceCache = new ReferenceCache(databaseContext, applicationModel);
            // Find attributes with references
            setReferencesIndices(datasetId);
        }
        public void PrepareOneRowForClient(IBaseModelWithApplicationAndData model)
        {
            var preparedDictionary = model.DataDictionary;
            translateIDToText(preparedDictionary);
            model.Data = JsonConvert.SerializeObject(preparedDictionary);
        }
        void setReferencesIndices(long datasetId)
        {
            referenceIndexTypeTuple = new List<(string Name, string Type)>();
            List<AttributeDescriptor> attributes = new List<AttributeDescriptor>();
            if (datasetId == (long)SystemDatasetsEnum.Users)
                attributes = applicationModel.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes;
            else
                attributes = applicationModel.ApplicationDescriptor.Datasets.Where(d => d.Id == datasetId).First().Attributes;
            foreach (var attribute in attributes)
            {
                bool isReference = !AttributeType.Types.Contains(attribute.Type);
                if (isReference)
                    referenceIndexTypeTuple.Add((attribute.Name, attribute.Type));
            }
        }
        void translateIDToText(Dictionary<string, List<object>> row)
        {
            foreach (var attribute in referenceIndexTypeTuple)
            {
                var ids = row[attribute.Name];
                row[attribute.Name] = new List<object>();
                if (ids.Count == 0)
                    continue;
                var lastId = ids.Last();
                foreach (var stringId in ids)
                {
                    long id;
                    if (long.TryParse(stringId.ToString(), out id))
                    {
                        string value = "";
                        value += referenceCache.GetTextForReference(attribute.Type, id);
                        row[attribute.Name].Add( new Tuple<string, string>(id.ToString(), value) );
                        if (!stringId.Equals(lastId))
                            value += ", ";
                    }
                }
            }
        }
    }
}