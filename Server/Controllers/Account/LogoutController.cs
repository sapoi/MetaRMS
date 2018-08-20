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

namespace Server.Controllers.Account
{
    [Route("api/account/[controller]")]
    public class LogoutController : Controller
    {
        private readonly IConfiguration _configuration;
        public LogoutController(IConfiguration configuration, DatabaseContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        private readonly DatabaseContext _context;

        // sem se dostane kdokoli
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            //TODO 
            // Have DB of no longer active tokens that still have some time to live
            // Query provided token against The Blacklist on every authorized request
            return Ok();
        }
    }
}