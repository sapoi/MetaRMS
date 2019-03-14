using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Structures;

namespace SharedLibrary.Services
{
    /// <summary>
    /// IRightsServece is a interface for services regarding to RightsModel.
    /// Additional comments of each method can be found with the implementation.
    /// </summary>
    public interface IRightsService  
    {           
        Task<HttpResponseMessage> GetAll(JWTToken token);
        Task<HttpResponseMessage> GetById(long rightsId, JWTToken token);
        Task<HttpResponseMessage> DeleteById(long rightsId, JWTToken token);
        Task<HttpResponseMessage> Put(RightsModel rightsModel, JWTToken token);
        Task<HttpResponseMessage> Create(RightsModel rightsModel, JWTToken token);
    }
}