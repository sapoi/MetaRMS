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
    public class LogoutModel : PageModel
    {
        private readonly IAccountService _accountService;

        public LogoutModel(IAccountService accountService)
        {
            this._accountService = accountService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // validation
            var token = AccessHelper.GetTokenFromPageModel(this);
            // if token is not valid, return to login page
            if (token == null)
                return RedirectToPage("/Account/Login");

            // logout on server
            await _accountService.Logout(token.Value);
            // clear cookies on client
            foreach (var cookieKey in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookieKey);
            }
            // redirect to index page with login
            return RedirectToPage("/Index");
        }
    }
}
