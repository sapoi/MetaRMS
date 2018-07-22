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
    public class SecretService : ISecretService
    {
        private HttpClient _client;
        public SecretService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:5000/api/secret");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            //_client.DefaultRequestHeaders.Add("Authorization", headerValue);
        }

        // GET password  protected controller on server side with JWT token
        public async Task<HttpResponseMessage> Get(string token)
        {
            // adding JWT token value to authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // GET request to server with authorization header containing JWT token value
            var response = await _client.GetAsync(_client.BaseAddress);


            return response;
        }
    }
}