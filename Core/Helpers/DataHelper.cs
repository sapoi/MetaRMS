using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Core;
using Core.Cache;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using System.Text;

namespace SharedLibrary.Helpers
{
    /// <summary>
    /// DataHelper is a helper class used by the server application with access to the database.
    /// It is used for preparing the data before they are sent to the client applications by
    /// adding text representations to references within the data.
    /// </summary>
    public class DataHelper
    {
        /// <summary>
        /// Tuple containing name and type of all reference type attributes for given dataset.
        /// </summary>
        List<(string Name, string Type)> referenceIndexTypeTuple;
        /// <summary>
        /// Model of application the data belongs to
        /// </summary>
        ApplicationModel applicationModel;
        /// <summary>
        /// Cache for reading and storing references
        /// </summary>
        ReferenceCache referenceCache;
        /// <summary>
        /// DataHelper constructor is responsible for preparing reference cache and stting reference indices.
        /// </summary>
        /// <param name="databaseContext">Database context used to access the database</param>
        /// <param name="applicationModel">Model of application</param>
        /// <param name="datasetId">Id of dataset the helper will work with</param>
        public DataHelper(DatabaseContext databaseContext, ApplicationModel applicationModel, long datasetId)
        {
            this.applicationModel = applicationModel;
            // Prepare reference cache
            this.referenceCache = new ReferenceCache(databaseContext, applicationModel);
            // Find attributes with references for dataset
            setReferencesIndices(datasetId);
        }
        /// <summary>
        /// This method adds translations of id references to the model orom the parametres.
        /// </summary>
        /// <param name="model">Model to have the data translated</param>
        public void PrepareOneRecordForClient(IBaseModelWithApplicationAndData model)
        {
            var preparedDictionary = model.DataDictionary;
            // Add translations to reference ids of the record
            translateIDToText(preparedDictionary);
            // Serialize data with the added translations
            model.Data = JsonConvert.SerializeObject(preparedDictionary);
        }
        /// <summary>
        /// This method fills referenceIndexTypeTuple for given dataset with tuples of 
        /// attribute name-attribute type for all reference type attributes.
        /// </summary>
        /// <param name="datasetId">Id of dataset to set the references for</param>
        void setReferencesIndices(long datasetId)
        {
            referenceIndexTypeTuple = new List<(string Name, string Type)>();
            // Get dataset attributes
            List<AttributeDescriptor> attributes = new List<AttributeDescriptor>();
            if (datasetId == (long)SystemDatasetsEnum.Users)
                attributes = applicationModel.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes;
            else
                attributes = applicationModel.ApplicationDescriptor.Datasets.Where(d => d.Id == datasetId).First().Attributes;
            // Find attributes of reference type
            foreach (var attribute in attributes)
            {
                if (!AttributeType.Types.Contains(attribute.Type))
                    referenceIndexTypeTuple.Add((attribute.Name, attribute.Type));
            }
        }
        /// <summary>
        /// This method adds text representation to reference ids with the help of reference cache.
        /// </summary>
        /// <param name="record">Record to get the translation for</param>
        void translateIDToText(Dictionary<string, List<object>> record)
        {
            // Go through all references in dataset
            foreach (var attribute in referenceIndexTypeTuple)
            {
                // Get references from record attribute
                var referenceIds = record[attribute.Name];
                // Clear the value of record attribute to prepare it for new values
                record[attribute.Name] = new List<object>();
                // No references to translate
                if (referenceIds.Count == 0)
                    continue;
                // Translate each reference
                foreach (var referenceId in referenceIds)
                {
                    long parsedId;
                    if (long.TryParse(referenceId.ToString(), out parsedId))
                    {
                        // Get translation for one reference from the reference cache
                        string value = referenceCache.GetTextForReference(attribute.Type, parsedId);
                        // Add text representation to the reference id
                        record[attribute.Name].Add( new Tuple<string, string>(parsedId.ToString(), value) );
                        if (!referenceId.Equals(referenceIds.Last()))
                            value += ", ";
                    }
                    // Log errors
                    else
                        Logger.LogToConsole($"ERROR: Id {referenceId} could not be parsed in DataHelper.");
                }

            }
        }
    }
}