using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Server;
using Server.Cache;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;

namespace SharedLibrary.Helpers
{
    public class DataHelper
    {
        List<(string Name, string Type)> referenceIndexTypeTuple;
        ApplicationModel applicationModel;
        ReferenceCache referenceCache;
        // DatabaseContext databaseContext;

        public DataHelper(DatabaseContext databaseContext, ApplicationModel applicationModel, long datasetId)
        {
            this.applicationModel = applicationModel;
            // this.databaseContext = databaseContext;
            this.referenceCache = new ReferenceCache(databaseContext, applicationModel);
            // find attributes with references
            setReferencesIndices(datasetId);
        }
        public void PrepareOneRowForClient(IBaseModelWithApplicationAndData model)
        {
            var preparedDictionary = model.DataDictionary;
            translateIDToText(preparedDictionary);
            // foreach (var item in model.DataDictionary)
            // {
            //     if (applicationModel.ApplicationDescriptor. item.Key)
            // }
            model.Data = JsonConvert.SerializeObject(preparedDictionary);
        }
        // public List<Dictionary<String, List<Object>>> PrepareForClient(List<BaseModelWithApplicationAndData> data, long datasetId)
        // {
        //     List<Dictionary<String, List<Object>>> modifiedData = new List<Dictionary<String, List<Object>>>();
        //     // find attributes with references
        //     setReferencesIndices(datasetId);
        //     // take each row from data
        //     foreach (var row in data)
        //     {
        //         var modifiedDataRow = row.DataDictionary;
        //         // add text representation for references
        //         translateIDToText(modifiedDataRow);
        //         // add dbID
        //         addDBId(modifiedDataRow, row.Id);
        //         // id dataset is SystemDatasetsEnum.Users
        //         if (datasetId == (long)SystemDatasetsEnum.Users)
        //         {
        //             // add rights with id
        //             modifiedDataRow.Add("RIGHTSId", new List<object> { new Tuple<string, string>( ((UserModel)row).RightsId.ToString(), ((UserModel)row).Rights.Name) } );
        //         }
        //         modifiedData.Add(modifiedDataRow);
        //     }
        //     return modifiedData;
        // }
        // public Dictionary<String, List<Object>> PrepareOneRowForClient(ref BaseModel row, long datasetId)
        // {
        //     setReferencesIndices(datasetId);

        //     // take each row from data
        //     foreach (var row in data)
        //     {
        //         // add text representation for references
        //         translateIDToText(row.DataDictionary);
        //         // add dbID
        //         addDBId(row.DataDictionary, row.Id);
        //     }
        // }
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
                        value += referenceCache.getTextForReference(attribute.Type, id);
                        row[attribute.Name].Add( new Tuple<string, string>(id.ToString(), value) );
                        if (!stringId.Equals(lastId))
                            value += ", ";
                    }
                }
            }
        }
        // void addDBId(Dictionary<string, List<object>> row, long id)
        // {
        //     // serializing list containing DBId, because every data is expected to be in a list
        //     row.Add("DBId", new List<object>() { id } );
        // }
    }
}