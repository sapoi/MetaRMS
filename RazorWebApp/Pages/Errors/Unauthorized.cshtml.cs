using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using SharedLibrary.Services;
using SharedLibrary.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using SharedLibrary.Descriptors;
using RazorWebApp.Helpers;
using SharedLibrary.Enums;

namespace RazorWebApp.Pages.Errors
{
    public class UnauthorizedModel : PageModel
    {
        public UnauthorizedModel() { }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
    }
}
