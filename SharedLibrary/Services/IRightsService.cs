using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    public interface IRightsService  
    {           
        Task<HttpResponseMessage> GetAll(string appName, string token);
        Task<HttpResponseMessage> GetById(string appName, long id, string token);
        Task<HttpResponseMessage> DeleteById(string appName, long id, string token);
        Task<HttpResponseMessage> PatchById(string appName, long id, RightsModel patchedRightsModel, string token);
        Task<HttpResponseMessage> Create(string appName, RightsModel newRightsModel, string token);
        

    }
}