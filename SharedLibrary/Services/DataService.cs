using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedLibrary.Models;
using SharedLibrary.Structures;

namespace SharedLibrary.Services
{
    public class DataService : BaseService, IDataService
    {
        public DataService() : base()
        {
            client.BaseAddress = new Uri(baseAddress + "data");
        }
        /// <summary>
        /// This method sends HTTP GET request with dataset id in URL paramters to get all data from the dataset.
        /// </summary>
        /// <param name="datasetId">Id of dataset to get the data from</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> GetAll(long datasetId, JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/get/" + datasetId);
            return await client.GetAsync(address);
        }
        /// <summary>
        /// This method sends HTTP GET request with dataset id and data id in URL paramters to get 
        /// data from dataset with datasetId and dataId.
        /// The datasetId here may seem as a bit redundant information, but is used on server to chech 
        /// request validity - dataId must have datasetId as dataset id.
        /// </summary>
        /// <param name="datasetId">Id of dataset to get the data from</param>
        /// <param name="dataId">Id of data to get</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> GetById(long datasetId, long dataId, JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/get/" + datasetId + "/" + dataId);
            return await client.GetAsync(address);
        }
        /// <summary>
        /// This method sends HTTP DELETE request with dataset id and data id in URL paramters to delete
        /// data from dataset with datasetId and dataId.
        /// The datasetId here may seem as a bit redundant information, but is used on server to chech 
        /// request validity - dataId must have datasetId as dataset id.
        /// </summary>
        /// <param name="datasetId">Id of dataset to dalate the data from</param>
        /// <param name="dataId">Id of data to delete</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> DeleteById(long datasetId, long dataId, JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/delete/"  + datasetId + "/" + dataId);
            return await client.DeleteAsync(address);
        }
        /// <summary>
        /// This method sends HTTP PUT request with modified DataModel to the server to modify already existing DataModel 
        /// record in the database. Existence of the model is tested by looking for combination of the same application 
        /// id, dataset id and id.
        /// </summary>
        /// <param name="dataModel">Modified DataModel</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> Put(DataModel dataModel, JWTToken token)
        {
            // Serialize DataModel
            string jsonData = JsonConvert.SerializeObject(dataModel);
            var jsonDataContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/put/");
            return await client.PutAsync(address, jsonDataContent);
        }
        /// <summary>
        /// This method sends HTTP POST request with new DataModel to the server to create a new DataModel recored in 
        /// the database. The new model must have application id and dataset id filled.
        /// </summary>
        /// <param name="dataModel">New DataModel</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> Create(DataModel dataModel, JWTToken token)
        {
            // Serialize DataModel
            string jsonData = JsonConvert.SerializeObject(dataModel);
            var jsonDataContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/create/");
            return await client.PostAsync(address, jsonDataContent);
        }
    }
}