using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedLibrary.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using SharedLibrary.Structures;
using RazorWebApp.Helpers;
using Microsoft.AspNetCore.Authentication;

namespace RazorWebApp.Pages.Account
{
    /// <summary>
    /// The LogoutModel class in Core.Pages.Account namespace is used as support for Logout.cshtml page. 
    /// The page is used to log out a logged user.
    /// </summary>
    public class LogoutModel : PageModel
    {
        /// <summary>
        /// Service for user account based requests to the server.
        /// </summary>
        private readonly IAccountService accountService;
        /// <summary>
        /// Constructor for initializing services and cache.
        /// </summary>
        /// <param name="accountService">Account service to be used</param>
        public LogoutModel(IAccountService accountService)
        {
            this.accountService = accountService;
        }
        /// <summary>
        /// This method is used when there is a GET request to Account/Logout.cshtml page
        /// </summary>
        /// <returns>Redirect to index page after user is logged out.</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Index");
            // Logout on server
            await accountService.Logout(token);
            // Clear cookies on client
            foreach (var cookieKey in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookieKey);
            }
            // Redirect to index page with login
            return RedirectToPage("/Index");
        }
    }
}
