using System;
using System.IO;
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
    public class AppInitService : BaseService, IAppInitService
    {
        public AppInitService() : base()
        {
            client.BaseAddress = new Uri(baseAddress + "appinit");
        }

        public async Task<HttpResponseMessage> InitializeApplication(string email, IFormFile file)
        {
            Stream stream = new MemoryStream();
            if (file != null)
                stream = file.OpenReadStream();
            var data = new MultipartFormDataContent
            {
                {new StringContent(email), "email"},
                {new StreamContent(stream), "file", "file"}
            };
            return await client.PostAsync(client.BaseAddress, data);
        }
    }
}