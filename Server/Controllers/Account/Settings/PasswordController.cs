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
        public async Task<IActionResult> Login([FromBody] PasswordChange passwords)
        {
            // get logged user's identity from HttpContext
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            // if user is authenticated and JWT contains claim named ApplicationName
            long userId = -1;
            if (!identity.IsAuthenticated || !identity.HasClaim(c => c.Type == "ApplicationName"
                                          || !identity.HasClaim(d => d.Type == "UserId"
                                          || !long.TryParse(identity.FindFirst("UserId").Value, out userId))))
                // user is not authorized to access application appName
                return Unauthorized(); //TODO zmanit na bad request s message
            string appName = identity.FindFirst("ApplicationName").Value;

            UserModel user = _context.UserDbSet.Where(u=> u.Application.LoginApplicationName == appName &&
                                                          u.Id == userId).FirstOrDefault();
            if (user.Password != PasswordHelper.ComputeHash(passwords.OldPassword))
                return BadRequest("Old password is incorrect.");
            
            user.Password = PasswordHelper.ComputeHash(passwords.NewPassword);
            await _context.SaveChangesAsync();
            return Ok("Password changed successfully.");
        }
    }
}