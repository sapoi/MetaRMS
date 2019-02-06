using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Server.Repositories;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using SharedLibrary.Structures;

namespace Server.Controllers.Account
{
    [Route("api/account/[controller]")]
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        public LoginController(IConfiguration configuration, DatabaseContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        private readonly DatabaseContext _context;
        // async Task<UserModel> getUserModel(LoginCredentials loginCredentials, ApplicationDescriptor applicationDescriptor)
        // {
        //     var userRepository = new UserRepository(_context);
        //     return userRepository.GetByApplicationLoginNameAndUsername(loginCredentials.ApplicationName, 
        //                                                                loginCredentials.Username);
        //     // return _context.UserDbSet.Where(u => (u.Application.LoginApplicationName == loginCredentials.ApplicationName && 
        //     //                                       u.GetUsername() == loginCredentials.Username)).FirstOrDefault();
        // }
        // async Task<ApplicationModel> getApplicationModelByLoginApplicationName(string loginApplicationName)
        // {
        //     return _context.ApplicationDbSet.Where(a => a.LoginApplicationName == loginApplicationName).FirstOrDefault();
        // }

        // sem se dostane kdokoli
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LoginCredentials loginCredentials)
        {
            if (ModelState.IsValid)
            {
                if (loginCredentials.LoginApplicationName == null)
                    return BadRequest($"ERROR: Application name is required.");
                if (loginCredentials.Username == null)
                    return BadRequest($"ERROR: Username is required.");
                if (loginCredentials.Password == null)
                    return BadRequest($"ERROR: Password is required.");
                var applicationRepository = new ApplicationRepository(_context);
                var applicationModel = applicationRepository.GetByLoginApplicationName(loginCredentials.LoginApplicationName);
                if (applicationModel == null)
                    return BadRequest($"ERROR: Invalid application name.");
                // var user = await getUserModel(loginCredentials, applicationModel.ApplicationDescriptor);
                var userRepository = new UserRepository(_context);
                var user = userRepository.GetByApplicationIdAndUsername(applicationModel.Id, loginCredentials.Username);
                if (user == null)
                    return BadRequest($"ERROR: User {loginCredentials.Username} does not exist in application {loginCredentials.LoginApplicationName}."); //"kombinace jmena aplikace a username"
                if (!PasswordHelper.CheckHash(loginCredentials.Password, user.Password))
                {
                    return BadRequest($"ERROR: Could not log in: combination of application name {loginCredentials.LoginApplicationName}, username {loginCredentials.Username} and password does not exist.");
                }
                // a když jsou platné přihlašovací údaje, vytvoří se token
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, loginCredentials.Username),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("ApplicationId", user.ApplicationId.ToString()),
                    new Claim("LoginApplicationName", loginCredentials.LoginApplicationName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken
                (
                    issuer: _configuration["TokenAuthentication:Issuer"],
                    audience: _configuration["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(60), //TODO
                    notBefore: DateTime.UtcNow,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["TokenAuthentication:SecretKey"])),
                            SecurityAlgorithms.HmacSha256)
                );
                //var tmptoken = new JwtSecurityTokenHandler().WriteToken(token);
                // a ten token se mu pošle zpátky
               return Ok(new { Value = new JwtSecurityTokenHandler().WriteToken(token) });
               //return token;
            }

            //return null;
            return BadRequest("ERROR: Could not create token - model state not valid");
        }
    }
}