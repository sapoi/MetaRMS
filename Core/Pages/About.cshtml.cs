using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Core.Pages
{
    /// <summary>
    /// The AboutModel class in Core.Pages namespace is used as support for About.cshtml page. 
    /// The page is used to display information about MetaRMS.
    /// </summary>
    public class AboutModel : PageModel
    {
        /// <summary>
        /// This method is used when there is a GET request to About.cshtml page.
        /// </summary>
        /// <returns>The page.</returns>
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
