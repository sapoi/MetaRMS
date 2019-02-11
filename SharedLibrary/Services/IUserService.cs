using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    /// <summary>
    /// IUserServece is a interface for services regarding to UserModel.
    /// Additional comments of each method can be found with the implementation.
    /// </summary>
    public interface IUserService  
    {           
        Task<HttpResponseMessage> GetAll(string token);
        Task<HttpResponseMessage> GetById(long id, string token);
        Task<HttpResponseMessage> DeleteById(long id, string token);
        Task<HttpResponseMessage> Patch(UserModel patchedUserModel, string token);
        Task<HttpResponseMessage> Create(UserModel newUserModel, string token);
        Task<HttpResponseMessage> ResetPasswordById(long id, string token);

    }
}