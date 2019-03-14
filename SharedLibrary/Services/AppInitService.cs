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
        /// <summary>
        /// This method sends HTTP POST request with email and file containing application descriptor
        /// in JSON format to the server to create a new application.
        /// </summary>
        /// <param name="email">Email to sent the login credentials to</param>
        /// <param name="file">File with application descriptor in JSON format</param>
        /// <returns>Response from the server.</returns>
        public async Task<HttpResponseMessage> InitializeApplication(string email, IFormFile file)
        {
            // Create new empty stream
            Stream stream = new MemoryStream();
            // If file is not empty, read into the stream
            if (file != null)
                stream = file.OpenReadStream();
            // Create multipart content
            var data = new MultipartFormDataContent
            {
                {new StringContent(email), "email"},
                {new StreamContent(stream), "file", "file"}
            };
            return await client.PostAsync(client.BaseAddress, data);
        }
    }
}