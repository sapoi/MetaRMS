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
    public class RightsService : BaseService, IRightsService
    {
        public RightsService() : base()
        {
            client.BaseAddress = new Uri(baseAddress + "rights");
        }
        /// <summary>
        /// This method sends HTTP GET request to get all rights from the database for application 
        /// from which is user sending the request.
        /// </summary>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> GetAll(JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/get/");
            return await client.GetAsync(address);
        }
        /// <summary>
        /// This method sends HTTP GET request with rights id in URL paramters to get rights with that id from the database.
        /// </summary>
        /// <param name="rightsI">Id of the rights to get</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> GetById(long rightsI, JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/get/" + rightsI);
            return await client.GetAsync(address);
        }
        /// <summary>
        /// This method sends HTTP DELETE request with rights id in URL paramters to delete rights with rightsId from the database.
        /// </summary>
        /// <param name="rightsId">Id of the rights to delete</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> DeleteById(long rightsId, JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/delete/" + rightsId);
            return await client.DeleteAsync(address);
        }
        /// <summary>
        /// This method sends HTTP PUT request with modified RightsModel to the server to modify already existing RightsModel 
        /// record in the database. Existence of the model is tested by looking for combination of the same application 
        /// id and rights id.
        /// </summary>
        /// <param name="rightsModel">Modified RightsModel</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> Put(RightsModel rightsModel, JWTToken token)
        {
            // Serialize RightsModel
            string jsonData = JsonConvert.SerializeObject(rightsModel);
            var jsonDataContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/put/");
            return await client.PutAsync(address, jsonDataContent);
        }
        /// <summary>
        /// This method sends HTTP POST request with new RightsModel to the server to create a new RightsModel recored in 
        /// the database. The new model must have application id filled.
        /// </summary>
        /// <param name="rightsModel">New RightsModel</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> Create(RightsModel rightsModel, JWTToken token)
        {
            // Serialize RightsModel
            string jsonData = JsonConvert.SerializeObject(rightsModel);
            var jsonDataContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/create/");
            return await client.PostAsync(address, jsonDataContent);
        }
    }
}