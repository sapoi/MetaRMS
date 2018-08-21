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
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                if (FileUpload == null)
                {
                    Message = "File with application description is required.";
                    return Page();
                }
                // 
                var response = await _appInitService.InitApp(Email, FileUpload);
                string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                // remove " form beginning and end of message
                Message = message.Substring(1, message.Length - 2);
                if (!response.IsSuccessStatusCode)
                    return Page();
                else
                    return RedirectToPage("/Index", new {message = Message});
            }
            //TODO vypsat nejakou chybu
            return Page();
        }
    }
}
