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
    public class LoginController : Controller
    {
        [Authorize]
        [HttpGet]
        public IActionResult Login()
        {
            return Ok(new LoginCredentials {ApplicationName = "applicationName", Username = "username", Password = "password"});
            //return View();
        }

        // [HttpPost]
        // public async Task<IActionResult> Login([FromBody] LoginCredentials loginCredentials)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest(ModelState);

        //     if (LoginUser(loginCredentials.Username, loginCredentials.Password))
        //     {
        //         var claims = new List<Claim>
        //         {
        //             new Claim(ClaimTypes.Name, loginCredentials.Username)
        //         };

        //         var userIdentity = new ClaimsIdentity(claims, "login");

        //         ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
        //         await HttpContext.SignInAsync(principal);

        //         //Just redirect to our index after logging in. 
        //         return Redirect("/");
        //     }
        //     return View();
        // }

        // private readonly UserManager<IdentityUser> _userManager;
        // private readonly SignInManager<IdentityUser> _signInManager;
        // public LoginController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        // {
        //     this._userManager = userManager;
        //     this._signInManager = signInManager;
        // }
        private readonly IConfiguration _configuration;
        public LoginController(IConfiguration configuration, DatabaseContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        private readonly DatabaseContext _context;
        async Task<bool> IsAuthenticated(LoginCredentials loginCredentials)
        {
            return (_context.UsersDbSet.Where(u => (u.ApplicationName == loginCredentials.ApplicationName && 
                                                    u.Username == loginCredentials.Username && 
                                                    u.Password == loginCredentials.Password)).Count() 
                                                        == 1);
        }

        // sem se dostane kdokoli
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginCredentials loginCredentials)
        {
            if (ModelState.IsValid)
            {
                var isAuthenticated = await IsAuthenticated(loginCredentials);
                if (!isAuthenticated)
                {
                    return Unauthorized();
                    //return null;
                }
                // a když jsou platné přihlašovací údaje, vytvoří se token
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, loginCredentials.Username),
                    new Claim("ApplicationName", loginCredentials.ApplicationName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken
                (
                    issuer: _configuration["TokenAuthentication:Issuer"],
                    audience: _configuration["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(60),
                    notBefore: DateTime.UtcNow,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["TokenAuthentication:SecretKey"])),
                            SecurityAlgorithms.HmacSha256)
                );
                var tmptoken = new JwtSecurityTokenHandler().WriteToken(token);
                // a ten token se mu pošle zpátky
               return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
               //return token;
            }

            //return null;
            return BadRequest("Could not create token");



            // var user = await _userManager.FindByEmailAsync(email);
            // if (user == null)
            // {
            //     ModelState.AddModelError(string.Empty, "Invalid login");
            //     return View();
            // }
            // if (!user.EmailConfirmed)
            // {
            //     ModelState.AddModelError(string.Empty, "Confirm your email first");
            //     return View();
            // }

            // var passwordSignInResult = await _signInManager.PasswordSignInAsync(user, password, isPersistent: rememberMe, lockoutOnFailure: false);

            // if (!passwordSignInResult.Succeeded)
            // {
            //     await _userManager.AccessFailedAsync(user);
            //     ModelState.AddModelError(string.Empty, "Invalid login");
            //     return View();
            // }

            // return Redirect("~/");
        }
    }
}