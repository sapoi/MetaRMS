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
    public class AccountService : IAccountService
    {
        private HttpClient _client;
        public AccountService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:5000/api/login");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<HttpResponseMessage> Login(LoginCredentials loginCredentials)
        {
            // odesílání přihlašovacích údalů na server
            string jsonLoginData = JsonConvert.SerializeObject(loginCredentials);
            var jsonLoginDataContent = new StringContent(jsonLoginData, Encoding.UTF8, "application/json");
            return await _client.PostAsync(_client.BaseAddress, jsonLoginDataContent);
        }
        public async Task<IActionResult> Logout()
        {
            // var address = new Uri("http://localhost:5000/api/logout");
            // return await _client.PostAsync(address, );
            return null;
        }
        public async Task<HttpResponseMessage> GetApplicationDescriptorByAppName(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var address = new Uri("http://localhost:5000/api/applicationdescriptor");
            return await _client.GetAsync(address);
        }
        public async Task<HttpResponseMessage> GetRightsByUserId(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var address = new Uri("http://localhost:5000/api/account/rights");
            return await _client.GetAsync(address);
        }
    }
}