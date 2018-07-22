using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    public interface IDataService  
    {           
        Task<HttpResponseMessage> GetAll(string appName, string dataset, string token);
        Task<HttpResponseMessage> DeleteById(string appName, string dataset, long id, string token);
    }
}