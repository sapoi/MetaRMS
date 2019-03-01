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
    public class AccountService : BaseService, IAccountService
    {
        public AccountService() : base()
        {
            client.BaseAddress = new Uri(baseAddress + "account");
        }

        public async Task<HttpResponseMessage> Login(LoginCredentials loginCredentials)
        {
            // odesílání přihlašovacích údalů na server
            string jsonLoginData = JsonConvert.SerializeObject(loginCredentials);
            var jsonLoginDataContent = new StringContent(jsonLoginData, Encoding.UTF8, "application/json");
            var address = new Uri(client.BaseAddress.OriginalString + "/login");
            return await client.PostAsync(address, jsonLoginDataContent);
        }
        public async Task<HttpResponseMessage> Logout(string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var address = new Uri(client.BaseAddress.OriginalString + "/logout");
            return await client.PostAsync(address, new StringContent(""));
        }
        public async Task<HttpResponseMessage> GetApplicationDescriptor(string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var address = new Uri(client.BaseAddress.OriginalString + "/applicationdescriptor");
            return await client.GetAsync(address);
        }
        public async Task<HttpResponseMessage> GetRights(string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var address = new Uri(client.BaseAddress.OriginalString + "/rights");
            return await client.GetAsync(address);
        }
        public async Task<HttpResponseMessage> ChangePassword(PasswordChangeStructure passwords, string token)
        {
            string jsonPasswordsData = JsonConvert.SerializeObject(passwords);
            var jsonDataContent = new StringContent(jsonPasswordsData, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var address = new Uri(client.BaseAddress.OriginalString + "/settings/password");
            return await client.PostAsync(address, jsonDataContent);
        }
    }
}