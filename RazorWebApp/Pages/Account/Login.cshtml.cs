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

namespace RazorWebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAccountService _accountService;

        public LoginModel(IAccountService accountService)
        {
            this._accountService = accountService;
        }

        [BindProperty]
        public LoginCredentials Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var aa = TempData.Peek("klic");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // zisk tokenu, pokud jsou přihlašovací údaje správné
                var response = await _accountService.Login(Input);
                var jsonToken = response.Content.ReadAsStringAsync().Result;

                if (!TempData.ContainsKey("klic"))
                    TempData.Add("klic", "hodnota");
                else 
                    TempData["klic"] = jsonToken;
            }
            return Page();
        }
    }
}
