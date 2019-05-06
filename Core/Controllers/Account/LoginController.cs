using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Structures;
using SharedLibrary.StaticFiles;

namespace Core.Controllers.Account
{
    [Route("api/account/[controller]")]
    public class LoginController : Controller
    {
        /// <summary>
        /// Application configuration properties
        /// </summary>
        private readonly IConfiguration configuration;
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;
        /// <summary>
        /// Constructor for initializing configuration and database context.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="context"></param>
        public LoginController(IConfiguration configuration, DatabaseContext context)
        {
            this.configuration = configuration;
            this.context = context;
        }
        /// <summary>
        /// API endpoint for log in.
        /// </summary>
        /// <param name="loginCredentials">Credentials to log int the application</param>
        /// <returns></returns>
        /// <response code="200">If user successfully authenticated</response>
        /// <response code="400">If input is not valid</response>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult Login([FromBody] LoginCredentials loginCredentials)
        {
            // List of messages to return to the client
            var messages = new List<Message>();

            // Input validation
            if (loginCredentials.LoginApplicationName == null)
                messages.Add(new Message(MessageTypeEnum.Error, 
                                         1001, 
                                         new List<string>()));
            if (loginCredentials.Username == null)
                messages.Add(new Message(MessageTypeEnum.Error, 
                                         1002, 
                                         new List<string>()));
            if (loginCredentials.Password == null)
                messages.Add(new Message(MessageTypeEnum.Error, 
                                         1003, 
                                         new List<string>()));
            if (messages.Count != 0)
                return BadRequest(messages);

            // Get application
            var applicationRepository = new ApplicationRepository(context);
            var applicationModel = applicationRepository.GetByLoginApplicationName(loginCredentials.LoginApplicationName);
            if (applicationModel == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                         1004, 
                                         new List<string>()));
                return BadRequest(messages);
            }
            
            // Get user
            var userRepository = new UserRepository(context);
            var user = userRepository.GetByApplicationIdAndUsername(applicationModel.Id, loginCredentials.Username);
            // User not found or password not correct
            if (user == null || !PasswordHelper.CheckHash(user.PasswordSalt, loginCredentials.Password, user.PasswordHash))
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                         1005, 
                                         new List<string>(){ loginCredentials.LoginApplicationName, loginCredentials.Username }));
                return BadRequest(messages);
            }

            // Login credentials are valid, create JWT token
            var claims = new[]
            {
                new Claim(Constants.JWTClaimUserId, user.Id.ToString()),
                new Claim(Constants.JWTClaimApplicationId, user.ApplicationId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken
            (
                issuer: configuration["TokenAuthentication:Issuer"],
                audience: configuration["TokenAuthentication:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenAuthentication:SecretKey"])),
                        SecurityAlgorithms.HmacSha256)
            );
            return Ok(new { Value = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}