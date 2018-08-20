using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Structures;

namespace SharedLibrary.Services
{
    public interface IAccountService  
    {           
        Task<HttpResponseMessage> Login(LoginCredentials data); 
        Task<HttpResponseMessage> Logout(string token); 
        Task<HttpResponseMessage> GetApplicationDescriptorByAppName(string token);
        Task<HttpResponseMessage> GetRightsByUserId(string token);
        // Task<HttpResponseMessage> ChangePassword(string appName, string oldPassword, string newPassword, string token);
    }
}