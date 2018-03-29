using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    public interface IAccountService  
    {           
        Task<HttpResponseMessage> Login(LoginCredentials data); 
        Task<IActionResult> Logout(); 
    }
}