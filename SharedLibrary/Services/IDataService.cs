using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    public interface IDataService  
    {           
        Task<HttpResponseMessage> GetAll(string appName, string dataset, string token);
        Task<HttpResponseMessage> GetById(string appName, string dataset, long id, string token);
        Task<HttpResponseMessage> DeleteById(string appName, string dataset, long id, string token);
        Task<HttpResponseMessage> PatchById(string appName, string dataset, long id, Dictionary<string, object> dataDict, string token);
        Task<HttpResponseMessage> Create(string appName, string dataset, Dictionary<string, object> dataDict, string token);
        

    }
}