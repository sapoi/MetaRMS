using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedLibrary.Models;

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
            //TODO
            return null;
        }
    }
}