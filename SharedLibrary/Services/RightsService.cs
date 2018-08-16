using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    public class RightsService : IRightsService
    {
        private HttpClient _client;
        public RightsService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:5000/api/rights");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<HttpResponseMessage> GetAll(string appName, string token)
        {
            // adding JWT token value to authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(_client.BaseAddress.OriginalString + "/get/" + appName);
            var response = await _client.GetAsync(address);
            return response;
        }

        public async Task<HttpResponseMessage> GetById(string appName, long id, string token)
        {
            // adding JWT token value to authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(_client.BaseAddress.OriginalString + "/get/" + appName + '/' + id);
            var response = await _client.GetAsync(address);

            return response;
        }

        public async Task<HttpResponseMessage> DeleteById(string appName, long id, string token)
        {
            // adding JWT token value to authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(_client.BaseAddress.OriginalString + "/delete/" + appName + '/' + id);
            var response = await _client.GetAsync(address);

            return response;
        }

        public async Task<HttpResponseMessage> PatchById(string appName, long id, RightsModel patchedRightsModel, string token)
        {
            // adding JWT token value to authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // odesílání dat na server
            string jsonData = JsonConvert.SerializeObject(patchedRightsModel);
            var jsonDataContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(_client.BaseAddress.OriginalString + "/patch/" + appName + '/' + id);
            var response = await _client.PostAsync(address, jsonDataContent);

            return response;
        }

        public async Task<HttpResponseMessage> Create(string appName, RightsModel newRightsModel, string token)
        {
            // adding JWT token value to authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // odesílání dat na server
            string jsonData = JsonConvert.SerializeObject(newRightsModel);
            var jsonDataContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(_client.BaseAddress.OriginalString + "/create/" + appName);
            var response = await _client.PostAsync(address, jsonDataContent);

            return response;
        }
    }
}