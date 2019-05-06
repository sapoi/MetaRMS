using System;
using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using System.Text;
using Core.Repositories;
using SharedLibrary.StaticFiles;
using SharedLibrary.Helpers;
using Newtonsoft.Json;

namespace Core.Cache
{
    /// <summary>
    /// This cache is used by DataHelper and it creates and stores text representation of references 
    /// that can be later send to the client.
    /// </summary>
    public class ReferenceCache
    {
        /// <summary>
        /// Model of application the data belongs to.
        /// </summary>
        ApplicationModel applicationModel;
        /// <summary>
        /// User repository used to access user data in the database.
        /// </summary>
        UserRepository userRepository;
        /// <summary>
        /// Data repository used to access dataset data in the database.
        /// </summary>
        DataRepository dataRepository;
        Dictionary<string, Dictionary<long, string>> referenceCache;
        /// <summary>
        /// ReferenceCache constructor prepares repositories and the cache.
        /// </summary>
        /// <param name="databaseContext">Database context used to access the database</param>
        /// <param name="applicationModel">Model of application</param>
        public ReferenceCache(DatabaseContext databaseContext, ApplicationModel applicationModel)
        {
            this.applicationModel = applicationModel;
            // Initialize necessary repositories
            this.userRepository = new UserRepository(databaseContext);
            this.dataRepository = new DataRepository(databaseContext);
            // Initialize cache
            this.referenceCache = new Dictionary<string, Dictionary<long, string>>();
        }
        /// <summary>
        /// This method creates text representation of a reference from database.
        /// </summary>
        /// <param name="referenceType">Type of the reference</param>
        /// <param name="referenceId">Id to get the text representation for</param>
        /// <param name="level">Current depth of looking for the reference</param>
        /// <returns>String representation of the reference.</returns>
        public string GetTextForReference(string referenceType, long referenceId, int level = 1)
        {
            // If text is already in cache, return it
            if (referenceCache.ContainsKey(referenceType) && referenceCache[referenceType].ContainsKey(referenceId))
               return referenceCache[referenceType][referenceId];
            // Otherwise load it into cache and return it
            string value = "";
            // If reference is of type system users dataset, get username as value
            if (referenceType == applicationModel.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name)
            {
                UserModel userModel = userRepository.GetById(referenceId);
                value = userModel.GetUsername();
                addToCache(referenceType, referenceId, value);
            }
            // If reference is of type user-defined dataset
            else
            {
                DataModel dataModel = dataRepository.GetById(referenceId);
                DatasetDescriptor datasetDescriptor = applicationModel.ApplicationDescriptor.Datasets.Where(d => d.Name == referenceType).FirstOrDefault();
                if (datasetDescriptor == null)
                    throw new Exception($"[ERROR]: Dataset {referenceType} not in application {applicationModel.LoginApplicationName} with id {applicationModel.Id}.\n");
                var sb = new StringBuilder("");
                // Get text representation for first at most MaxAttributesDisplayedInReference attributes of dataModel
                bool shouldAddToCache = true;
                var displayedAttributesCount = Math.Min(datasetDescriptor.Attributes.Count, Constants.MaxAttributesDisplayedInReference);
                for (int i = 0; i < displayedAttributesCount; i++)
                {
                    var attributeDescriptor = datasetDescriptor.Attributes[i];
                    // For basic types get the value and for reference types get first at most MaxReferencedDisplayedInListOfReferences references and get their text representation
                    foreach (var dataDictionaryValue in dataModel.DataDictionary[attributeDescriptor.Name].Take(Constants.MaxReferencedDisplayedInListOfReferences))
                    {
                        // If dataDictionaryValue is reference, get its representation
                        bool isReference = !AttributeType.Types.Contains(attributeDescriptor.Type);
                        if (isReference)
                        {
                            long dataId;
                            if (level > Constants.MaxDepthOfDisplayedReferences)
                            {
                                sb.Append("...");
                                // If reference is too deep, do not add it into the cache
                                shouldAddToCache = false;
                            }
                            else if (long.TryParse(dataDictionaryValue.ToString(), out dataId))
                            {
                                sb.Append("(" + GetTextForReference(attributeDescriptor.Type, dataId, level + 1) + ")");
                                // Do not add too deep references into the cache
                                if (level + 1 > Constants.MaxDepthOfDisplayedReferences)
                                    shouldAddToCache = false;
                            }
                            else
                            {
                                try
                                {
                                    sb.Append(JsonConvert.DeserializeObject<Tuple<string, string>>(dataDictionaryValue.ToString()).Item2);
                                    // Do not add too deep references into the cache
                                    if (level + 1 > Constants.MaxDepthOfDisplayedReferences)
                                        shouldAddToCache = false;
                                }
                                catch (Exception e)
                                {
                                    if (e is JsonSerializationException || e is JsonReaderException)
                                    {
                                        Logger.LogToConsole($"ERROR: Reference {dataDictionaryValue} could not be parsed in ReferenceCache.");
                                        Logger.LogExceptionToConsole(e);
                                        // Do not add invalid references into the cache
                                        shouldAddToCache = false;
                                    }
                                    else
                                        throw;
                                }
                            }
                        }
                        // If dataDictionaryValue is not reference, get its value
                        else
                        {
                            sb.Append(dataDictionaryValue);
                        }
                        // Separate values in one attribute with comma (except last value)
                        if (!dataDictionaryValue.Equals(dataModel.DataDictionary[attributeDescriptor.Name].Take(Constants.MaxReferencedDisplayedInListOfReferences).Last()))
                            sb.Append(", ");
                    }
                    // Separate values of individual attributes with | (except last attribute and empty attributes)
                    if (i + 1 != displayedAttributesCount && dataModel.DataDictionary[datasetDescriptor.Attributes[i + 1].Name].Count != 0)
                    {
                        sb.Append(" | ");
                    }
                }
                value = sb.ToString();
                if (shouldAddToCache)
                    addToCache(referenceType, referenceId, value);
            }
            return value;
        }
        /// <summary>
        /// This method adds found value to the cache.
        /// </summary>
        /// <param name="type">Type of the value</param>
        /// <param name="id">Id of the value</param>
        /// <param name="value">The value itself</param>
        void addToCache(string type, long id, string value)
        {
            // If cache does not already contain key for the value type
            if (!referenceCache.ContainsKey(type))
            {
                // Add new key-value pair under the newly added type
                referenceCache.Add(type, new Dictionary<long, string>() { { id, value } });
            }
            else if (!referenceCache[type].ContainsKey(id))
            {
                // Add new key-value pair under the type from the parameter
                referenceCache[type].Add(id, value);
            }
        }
    }
}