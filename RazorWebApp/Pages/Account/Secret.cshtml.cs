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
using Newtonsoft.Json;

namespace RazorWebApp.Pages.Account
{
    public class AccessToken
        {
            public string name;
            public string token;
        }
    public class SecretModel : PageModel
    {
        [BindProperty]
        public string SecretResponse { get; set; }
        private readonly ISecretService _secretService;
        public SecretModel(ISecretService secretService)
        {
            this._secretService = secretService;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var cookieData = (string)TempData.Peek("klic");

            var token = JsonConvert.DeserializeObject<AccessToken>(cookieData);

            // tady bych potřebovala odeslat token
            var response = await _secretService.Get(token.token);

            this.SecretResponse = await response.Content.ReadAsStringAsync();
            if (SecretResponse == null)
                SecretResponse = "null";

            return Page();
        }
    }
}
