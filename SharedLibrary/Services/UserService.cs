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
    public class UserService : BaseService, IUserService
    {
        public UserService() : base()
        {
            client.BaseAddress = new Uri(baseAddress + "user");
        }
        /// <summary>
        /// This method sends HTTP GET request to get all users from the database for application 
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
        /// This method sends HTTP GET request with user id in URL paramters to get user with that id from the database.
        /// </summary>
        /// <param name="userId">Id of the user to get</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> GetById(long userId, JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/get/" + userId);
            return await client.GetAsync(address);
        }
        /// <summary>
        /// This method sends HTTP DELETE request with user id in URL paramters to delete user with userId from the database.
        /// </summary>
        /// <param name="userId">Id of the user to delete</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> DeleteById(long userId, JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/delete/" + userId);
            return await client.DeleteAsync(address);
        }
        /// <summary>
        /// This method sends HTTP PUT request with modified UserModel to the server to modify already existing UserModel 
        /// record in the database. Existence of the model is tested by looking for combination of the same application 
        /// id and user id.
        /// </summary>
        /// <param name="userModel">Modified UserModel</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> Put(UserModel userModel, JWTToken token)
        {
            // Serialize UserModel
            string jsonData = JsonConvert.SerializeObject(userModel);
            var jsonDataContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/put/");
            return await client.PutAsync(address, jsonDataContent);
        }
        /// <summary>
        /// This method sends HTTP POST request with new UserModel to the server to create a new UserModel recored in 
        /// the database. The new model must have application id filled.
        /// </summary>
        /// <param name="userModel">New UserModel</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> Create(UserModel userModel, JWTToken token)
        {
            // Serialize UserModel
            string jsonData = JsonConvert.SerializeObject(userModel);
            var jsonDataContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/create/");
            return await client.PostAsync(address, jsonDataContent);
        }
        /// <summary>
        /// This method sends HTTP POST request with user id in URL paramters to the server to reset password of that user.
        /// </summary>
        /// <param name="userId">Id of the user to reset password</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> ResetPasswordById(long userId, JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/resetPassword/" + userId);
            return await client.PostAsync(address, null);
        }
    }
}