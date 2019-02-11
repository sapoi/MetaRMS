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
        private HttpClient client;
        public AppInitService()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000/api/appinit");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<HttpResponseMessage> InitializeApplication(string email, IFormFile file)
        {
            //TODO pada kdyz je file null
            //if (file == null)
            var data = new MultipartFormDataContent
            {
                {new StringContent(email), "email"},
                {new StreamContent(file.OpenReadStream()), "file", "file"}
            };
            return await client.PostAsync(client.BaseAddress, data);
        }
    }
}