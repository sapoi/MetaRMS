using System;
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
    public class AccountService : BaseService, IAccountService
    {
        public AccountService() : base()
        {
            client.BaseAddress = new Uri(baseAddress + "account");
        }
        /// <summary>
        /// This method sends HTTP POST request with login credentials to the server to log in the user 
        /// and receive a JWT token.
        /// </summary>
        /// <param name="loginCredentials">Credentials to sent to the server.</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> Login(LoginCredentials loginCredentials)
        {
            // Serialize login credentials
            string jsonLoginData = JsonConvert.SerializeObject(loginCredentials);
            var jsonLoginDataContent = new StringContent(jsonLoginData, Encoding.UTF8, "application/json");
            var address = new Uri(client.BaseAddress.OriginalString + "/login");
            return await client.PostAsync(address, jsonLoginDataContent);
        }
        /// <summary>
        /// This method sends HTTP POST request to log user out.
        /// </summary>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> Logout(JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/logout");
            return await client.PostAsync(address, new StringContent(""));
        }
        /// <summary>
        /// This method sends HTTP GET request to get application descriptor for logged user.
        /// </summary>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> GetApplicationDescriptor(JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/applicationdescriptor");
            return await client.GetAsync(address);
        }
        /// <summary>
        /// This method sends HTTP GET request to get RightsModel for logged user.
        /// </summary>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> GetRightsModel(JWTToken token)
        {
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/rights");
            return await client.GetAsync(address);
        }
        /// <summary>
        /// This method sends HTTP POST request with old and new passowrds in the PasswordChangeStructure 
        /// to the server to change user's password.
        /// </summary>
        /// <param name="passwords">PasswordChangeStructure containing old and new passwords</param>
        /// <param name="token">JWT authentication token</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> ChangePassword(PasswordChangeStructure passwords, JWTToken token)
        {
            // Serialize passwords
            string jsonPasswordsData = JsonConvert.SerializeObject(passwords);
            var jsonDataContent = new StringContent(jsonPasswordsData, Encoding.UTF8, "application/json");
            // Add JWT token value to the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            var address = new Uri(client.BaseAddress.OriginalString + "/settings/password");
            return await client.PostAsync(address, jsonDataContent);
        }
    }
}