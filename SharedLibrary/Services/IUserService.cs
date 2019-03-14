using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using SharedLibrary.Structures;

namespace SharedLibrary.Services
{
    /// <summary>
    /// IUserServece is a interface for services regarding to UserModel.
    /// Additional comments of each method can be found with the implementation.
    /// </summary>
    public interface IUserService  
    {           
        Task<HttpResponseMessage> GetAll(JWTToken token);
        Task<HttpResponseMessage> GetById(long userId, JWTToken token);
        Task<HttpResponseMessage> DeleteById(long userId, JWTToken token);
        Task<HttpResponseMessage> Put(UserModel userModel, JWTToken token);
        Task<HttpResponseMessage> Create(UserModel userModel, JWTToken token);
        Task<HttpResponseMessage> ResetPasswordById(long userId, JWTToken token);

    }
}