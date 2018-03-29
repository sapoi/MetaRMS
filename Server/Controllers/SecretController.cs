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
using SharedLibrary.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    public class SecretController : Controller
    {
        public SecretController(DatabaseContext context)
        {
            _context = context;
        }
        private readonly DatabaseContext _context;

        // semka by se měl dostat pouze uživatel s platným tokenem
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new Message { Text = "secret text"});
        }
        public class Message
        {
            public string Text { get; set; }
        }
    }
}