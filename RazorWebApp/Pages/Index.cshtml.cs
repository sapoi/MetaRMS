using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using RazorWebApp.Helpers;
using SharedLibrary.Services;
using SharedLibrary.Structures;

namespace RazorWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;

        public IndexModel(IAccountService accountService, IMemoryCache memoryCache)
        {
            this._accountService = accountService;
            this._cache = memoryCache;
        }

        [BindProperty]
        public LoginCredentials Input { get; set; }
        public string Message { get; set; }
        public IActionResult OnGet(string message = null)
        {
            // get AccessToken from PageModel
            var token = AccessHelper.GetTokenFromPageModel(this);
            // if there is no token, log info and redirect user to login page
            if (token == null)
            {
                Message = message;
                //TODO
                return Page();
            }
            return RedirectToPage("/Data/Get");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // zisk tokenu, pokud jsou přihlašovací údaje správné
                var response = await _accountService.Login(Input);
                if (!response.IsSuccessStatusCode)
                {
                    string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Message = message.Substring(1, message.Length - 2);
                    return Page();
                }
                var jsonToken = response.Content.ReadAsStringAsync().Result;
                // ulozeni tokenu do session storage
                HttpContext.Session.SetString("sessionJWT", jsonToken);
                var token = AccessHelper.GetTokenFromPageModel(this);
                // pokud jiz neni v cahe, nacist appliction descriptor a ulozit ho do ni
                await CacheAccessHelper.GetApplicationDescriptorFromCacheAsync(_cache, _accountService, token);

                return RedirectToPage("/Data/Get");
            }
            //TODO vypsat nejakou chybu
            return Page();
        }
    }
}
