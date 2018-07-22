using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using SharedLibrary.Services;
using System.Net;

namespace RazorWebApp.Pages.Appinit
{
        public class IndexModel : PageModel
    {
        private readonly IAppInitService _appInitService;
        private IMemoryCache _cache;

        public IndexModel(IAppInitService appInitService, IMemoryCache memoryCache)
        {
            this._appInitService = appInitService;
            this._cache = memoryCache;
        }

        [BindProperty]
        public IFormFile FileUpload { get; set; }

        [BindProperty]
        public string Email { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // 
                var response = await _appInitService.InitApp(Email, FileUpload);

                return RedirectToPage("/Account/Login");
            }
            //TODO vypsat nejakou chybu
            return Page();
        }
    }
}
