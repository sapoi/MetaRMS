using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using SharedLibrary.Services;
using SharedLibrary.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using Newtonsoft.Json;
using SharedLibrary.Descriptors;

namespace RazorWebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;

        public LoginModel(IAccountService accountService, IMemoryCache memoryCache)
        {
            this._accountService = accountService;
            this._cache = memoryCache;
        }

        [BindProperty]
        public LoginCredentials Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // zisk tokenu, pokud jsou přihlašovací údaje správné
                var response = await _accountService.Login(Input);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //TODO a vypsat chybovou hlasku
                    return Page();
                }
                var jsonToken = response.Content.ReadAsStringAsync().Result;
                // ulozeni tokenu do session storage
                HttpContext.Session.SetString("sessionJWT", jsonToken);
                var token = AuthorizationHelper.GetToken(this);
                // pokud jiz neni v cahe, nacist appliction descriptor a ulozit ho do ni
                await CacheAccessHelper.GetApplicationDescriptorFromCacheAsync(_cache, _accountService, Input.ApplicationName, token.token);
                //return RedirectToPage("/Account/Secret");
                return RedirectToPage("/Data/Get");
            }
            //TODO vypsat nejakou chybu
            return Page();
        }
    }
}
