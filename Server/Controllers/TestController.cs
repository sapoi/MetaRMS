// using System.Collections.Generic;
// using System.Linq;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;

// namespace Server.Controllers
// {
//     [Route("api/[controller]")]
//     public class TestController : Controller
//     {
//         private readonly DatabaseContext _context;

//         public TestController(DatabaseContext context)
//         {
//             _context = context;
//         }
//         bool IsAuthentic(string appName, string login, string password)
//         {
//             if (_context.UsersDbSet.Where(u => (u.ApplicationName == appName && u.Login == login && u.Password == password)).Count() == 1)
//                 return true;
//             return false;
//         }

//         [HttpPost]
//         public async Task<IActionResult> Login(string applicationName, string login, string password)
//         {
//             if (!IsAuthentic(applicationName, login, password))
//                 return View();

//             // create claims
//             List<Claim> claims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.Name, login),
//                 new Claim(ClaimTypes.Role, applicationName)
//             };

//             // create identity
//             ClaimsIdentity identity = new ClaimsIdentity(claims, "cookie");

//             // create principal
//             ClaimsPrincipal principal = new ClaimsPrincipal(identity);

//             // sign-in
//             await HttpContext.SignInAsync(
//                     scheme: CookieAuthenticationDefaults.AuthenticationScheme,
//                     principal: principal);
            
//             return NoContent();
//         }

//         public async Task<IActionResult> Logout()
//         {
//             await HttpContext.SignOutAsync(
//                     scheme: CookieAuthenticationDefaults.AuthenticationScheme);

//             return RedirectToAction("Login");
//         }

//         [HttpGet]
//         [Authorize]
//         public string Get()
//         {
//             return "secret string";
//         }
//     }
// }