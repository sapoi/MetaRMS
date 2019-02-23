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
    public class DataService : IDataService
    {
        private HttpClient client;
        public DataService()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://sapoi.aspifyhost.com/api/data");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<HttpResponseMessage> GetAll(long datasetId, string token)
        {
            // adding JWT token value to authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(client.BaseAddress.OriginalString + "/get/" + datasetId);
            var response = await client.GetAsync(address);


            return response;
        }

        public async Task<HttpResponseMessage> GetById(long datasetId, long id, string token)
        {
            // adding JWT token value to authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(client.BaseAddress.OriginalString + "/get/" + datasetId + "/" + id);
            var response = await client.GetAsync(address);

            return response;
        }

        public async Task<HttpResponseMessage> DeleteById(long id, string token)
        {
            // adding JWT token value to authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(client.BaseAddress.OriginalString + "/delete/" + id);
            var response = await client.GetAsync(address);

            return response;
        }

        public async Task<HttpResponseMessage> Patch(DataModel dataModel, string token)
        {
            // adding JWT token value to authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // odesílání dat na server
            string jsonData = JsonConvert.SerializeObject(dataModel);
            var jsonDataContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(client.BaseAddress.OriginalString + "/patch/");
            var response = await client.PostAsync(address, jsonDataContent);

            return response;
        }

        public async Task<HttpResponseMessage> Create(DataModel dataModel, string token)
        {
            // adding JWT token value to authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // odesílání dat na server
            string jsonData = JsonConvert.SerializeObject(dataModel);
            var jsonDataContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            // GET request to server with authorization header containing JWT token value
            var address = new Uri(client.BaseAddress.OriginalString + "/create/");
            var response = await client.PostAsync(address, jsonDataContent);

            return response;
        }
    }
}