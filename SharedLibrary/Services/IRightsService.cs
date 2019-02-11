using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    /// <summary>
    /// IRightsServece is a interface for services regarding to RightsModel.
    /// Additional comments of each method can be found with the implementation.
    /// </summary>
    public interface IRightsService  
    {           
        Task<HttpResponseMessage> GetAll(string token);
        Task<HttpResponseMessage> GetById(long id, string token);
        Task<HttpResponseMessage> DeleteById(long id, string token);
        Task<HttpResponseMessage> Patch(RightsModel patchedRightsModel, string token);
        Task<HttpResponseMessage> Create(RightsModel newRightsModel, string token);
    }
}