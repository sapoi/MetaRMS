using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Structures;

namespace SharedLibrary.Services
{
    /// <summary>
    /// IAccountService is a interface for services regarding to user account.
    /// Additional comments of each method can be found with the implementation.
    /// </summary>
    public interface IAccountService  
    {           
        Task<HttpResponseMessage> Login(LoginCredentials data); 
        Task<HttpResponseMessage> Logout(JWTToken token); 
        Task<HttpResponseMessage> GetApplicationDescriptor(JWTToken token);
        Task<HttpResponseMessage> GetRightsModel(JWTToken token);
        Task<HttpResponseMessage> ChangePassword(PasswordChangeStructure passwords, JWTToken token);
    }
}