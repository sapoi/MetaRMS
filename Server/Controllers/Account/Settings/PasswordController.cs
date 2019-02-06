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
using Server.Helpers;
using Server.Repositories;
using SharedLibrary.Helpers;
using SharedLibrary.Models;
using SharedLibrary.Structures;

namespace Server.Controllers.Account.Settings
{
    [Route("api/account/settings/[controller]")]
    public class PasswordController : Controller
    {
        private readonly IConfiguration _configuration;
        public PasswordController(IConfiguration configuration, DatabaseContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        private readonly DatabaseContext _context;

        [Authorize]
        [HttpPost]
        public IActionResult PasswordChange([FromBody] PasswordChangeStructure passwords)
        {
            var controllerHelper = new ControllerHelper(_context);
            // Authentication
            var authUserModel = controllerHelper.Authenticate(HttpContext.User.Identity as ClaimsIdentity);
            if (authUserModel == null)
                return Unauthorized();
            // Authorization - none - every logged user is authorized to change an own password

            //INPUT VALIDATIONS
            if (passwords.OldPassword == null || passwords.OldPassword == "" ||
                passwords.NewPassword == null || passwords.NewPassword == "" )
                return BadRequest("ERROR: Both old and new passwords are required to be non empty.");
            if (authUserModel.Password != PasswordHelper.ComputeHash(passwords.OldPassword))
                return BadRequest("ERROR: Old password is incorrect.");

            var userRepository = new UserRepository(_context);
            userRepository.SetPassword(authUserModel, passwords.NewPassword);
            return Ok("Password changed successfully.");



            // // get logged user's identity from HttpContext
            // var identity = HttpContext.User.Identity as ClaimsIdentity;
            // // if user is authenticated and JWT contains claim named LoginApplicationName
            // long userId = -1;
            // if (!identity.IsAuthenticated || !identity.HasClaim(c => c.Type == "LoginApplicationName"
            //                               || !identity.HasClaim(d => d.Type == "UserId"
            //                               || !long.TryParse(identity.FindFirst("UserId").Value, out userId))))
            //     // user is not authorized to access application appName
            //     return Unauthorized(); //TODO zmanit na bad request s message
            // string appName = identity.FindFirst("LoginApplicationName").Value;

            // //INPUT VALIDATIONS
            // if (passwords.OldPassword == null || passwords.OldPassword == "" ||
            //     passwords.NewPassword == null || passwords.NewPassword == "" )
            //     return BadRequest("Both old and new passwords are required to be non empty.");

            // var userRepository = new UserRepository(_context);
            // var user = userRepository.GetById(userId);
            // // UserModel user = _context.UserDbSet.Where(u=> u.Application.LoginApplicationName == appName &&
            // //                                               u.Id == userId).FirstOrDefault();
            // if (user.Password != PasswordHelper.ComputeHash(passwords.OldPassword))
            //     return BadRequest("Old password is incorrect.");
            // userRepository.SetPassword(user, passwords.NewPassword);
            // // user.Password = PasswordHelper.ComputeHash(passwords.NewPassword);
            // // await _context.SaveChangesAsync();
            // return Ok("Password changed successfully.");
        }
    }
}