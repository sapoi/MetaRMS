using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    public class AppInitService : IAppInitService
    {
        private HttpClient _client;
        public AppInitService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:5000/api/appinit");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<HttpResponseMessage> InitApp(string email, IFormFile file)
        {
            //TODO pada kdyz je file null
            //if (file == null)
            var data = new MultipartFormDataContent
            {
                {new StringContent(email), "email"},
                {new StreamContent(file.OpenReadStream()), "file", "file"}
            };
            return await _client.PostAsync(_client.BaseAddress, data);
        }
    }
}