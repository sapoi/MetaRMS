using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Core.Pages
{
    /// <summary>
    /// The ErrorModel class in Core.Pages namespace is used as support for Error.cshtml page. 
    /// The page is used to display server error annoucement.
    /// </summary>
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// Constructor for initializing model.
        /// </summary>
        public ErrorModel() { }
        /// <summary>
        /// This method is used when there is a GET request to Error.cshtml page.
        /// </summary>
        /// <returns>The page.</returns>
        public IActionResult OnGetAsync()
        {
            return Page();
        }
    }
}
