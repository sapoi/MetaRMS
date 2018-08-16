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
        Task<IActionResult> Logout(); 
        Task<HttpResponseMessage> GetApplicationDescriptorByAppName(string token);
        Task<HttpResponseMessage> GetRightsByUserId(string token);
    }
}