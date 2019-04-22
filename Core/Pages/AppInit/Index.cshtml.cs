using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using SharedLibrary.Services;
using System.Collections.Generic;
using SharedLibrary.Structures;
using Newtonsoft.Json;
using SharedLibrary.Helpers;
using SharedLibrary.Enums;

namespace RazorWebApp.Pages.Appinit
{
    /// <summary>
    /// The IndexModel class in Core.Pages.AppInit namespace is used as support for Index.cshtml page. 
    /// The page is used to create a new application.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>
        /// Service for application initialization request to the server.
        /// </summary>
        private readonly IAppInitService appInitService;
        /// <summary>
        /// In-memory cache service.
        /// </summary>
        private IMemoryCache cache;
        /// <summary>
        /// Constructor for initializing service and cache.
        /// </summary>
        /// <param name="appInitService">Application initialization service to be used</param>
        /// <param name="memoryCache">Cache to be used</param>
        public IndexModel(IAppInitService appInitService, IMemoryCache memoryCache)
        {
            this.appInitService = appInitService;
            this.cache = memoryCache;
        }
        /// <summary>
        /// File containing application descriptor in JSON fromat.
        /// </summary>
        /// <value>File in JSON format</value>
        [BindProperty]
        public IFormFile FileUpload { get; set; }
        /// <summary>
        /// Email address to send the login credentials to.
        /// </summary>
        /// <value>String email address</value>
        [BindProperty]
        public string Email { get; set; }
        /// <summary>
        /// Messages property contains list of messages for user.
        /// </summary>
        /// <value>List of Message structure</value>
        public List<Message> Messages { get; set; }
        /// <summary>
        /// This method is used when there is a GET request to AppInit/Index.cshtml page.
        /// </summary>
        /// <returns>The page.</returns>
        public IActionResult OnGetAsync()
        {
            // Messages
            Messages = new List<Message>();
            // Get messages from cookie
            var serializedMessages = TempData["Messages"];
            TempData.Remove("Messages");
            if (serializedMessages != null)
            {
                try
                {
                    Messages = JsonConvert.DeserializeObject<List<Message>>((string)serializedMessages) ?? throw new JsonSerializationException();
                }
                catch (JsonSerializationException e)
                {
                    Logger.LogToConsole($"Messages {serializedMessages} serialization failed when creating a new application.");
                    Logger.LogExceptionToConsole(e);
                }    
            }
            return Page();
        }
        /// <summary>
        /// OnPostAsync method is invoked after clicking on Submit button.
        /// </summary>
        /// <returns>Redirect to Index page or the same page with validation messages.</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            var messages = new List<Message>();

            // Check if file was posted
            if (FileUpload == null)
            {
                messages.Add(new Message(MessageTypeEnum.Error, 
                                         0001, 
                                         new List<string>()));
            }
            // Create request to the server via appInitService
            else
            {
                var response = await appInitService.InitializeApplication(Email, FileUpload);
                try
                {
                    // If response status code if successfull, parse messages and redirect to login page
                    if (response.IsSuccessStatusCode)
                    {
                        // Set messages to cookie
                        TempData["Messages"] = await response.Content.ReadAsStringAsync();
                        return RedirectToPage("/Index");
                    }
                    // Otherwise try parse error messages
                    else
                        messages = JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync()) ?? throw new JsonSerializationException();
                }
                catch (JsonSerializationException e)
                {
                    // In case of JSON parsing error, create server error message
                    messages.Add(MessageHepler.Create1007());
                    Logger.LogExceptionToConsole(e);
                }
            }
            Messages = messages;
            return Page();
        }
    }
}
