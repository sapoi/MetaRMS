using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SharedLibrary.Services
{
    /// <summary>
    /// IAppInitService is a interface for services regarding to new application initialization.
    /// Additional comments of each method can be found with the implementation.
    /// </summary>
    public interface IAppInitService  
    {           
        Task<HttpResponseMessage> InitializeApplication(string Email, IFormFile file); 
    }
}