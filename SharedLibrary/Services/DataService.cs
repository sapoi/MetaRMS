using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    public class DataService : IDataService
    {
        private HttpClient _client;
        public DataService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:5000/api");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<HttpResponseMessage> GetAll(string appName, string dataset, string token)
        {
            // adding JWT token value to authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(_client.BaseAddress.OriginalString + "/get/" + appName + '/' + dataset);
            var response = await _client.GetAsync(address);


            return response;
        }

        public async Task<HttpResponseMessage> DeleteById(string appName, string dataset, long id, string token)
        {
            // adding JWT token value to authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(_client.BaseAddress.OriginalString + "/delete/" + appName + '/' + dataset + '/' + id);
            var response = await _client.GetAsync(address);

            return response;
        }
    }
}