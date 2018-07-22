using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    public interface IAppInitService  
    {           
        Task<HttpResponseMessage> InitApp(string Email, IFormFile file); 
    }
}