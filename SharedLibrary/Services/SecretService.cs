using System;
using System.Net.Http;
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

        public async Task<HttpResponseMessage> Get(string cookieData)
        {
            // odesílání tokenu na server
            var headerValue = "Bearer " + cookieData;
            _client.DefaultRequestHeaders.Add("Authorization", headerValue);
            var response = await _client.GetAsync(_client.BaseAddress);


            return response;
        }
    }
}