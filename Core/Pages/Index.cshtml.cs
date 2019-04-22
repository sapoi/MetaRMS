using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SharedLibrary.Helpers;
using SharedLibrary.Services;
using SharedLibrary.Structures;
using SharedLibrary.StaticFiles;

namespace Core.Pages
{
    /// <summary>
    /// The IndexModel class in Core.Pages namespace is used as support for Index.cshtml page. 
    /// The page is used to display login form.
    /// </summary>
    public class IndexModel : PageModel
    {
        /// <summary>
        /// Service for user account based requests to the server.
        /// </summary>
        private readonly IAccountService accountService;
        /// <summary>
        /// In-memory cache service.
        /// </summary>
        private IMemoryCache cache;
        /// <summary>
        /// Constructor for initializing services and cache.
        /// </summary>
        /// <param name="accountService">Account service to be used</param>
        /// <param name="memoryCache">Cache to be used</param>
        public IndexModel(IAccountService accountService, IMemoryCache memoryCache)
        {
            this.accountService = accountService;
            this.cache = memoryCache;
        }
        /// <summary>
        /// Login credentials from input.
        /// </summary>
        /// <value>Login credentials class instance</value>
        [BindProperty]
        public LoginCredentials LoginCredentials { get; set; }
        /// <summary>
        /// Messages property contains list of messages for user.
        /// </summary>
        /// <value>List of Message structure</value>
        public List<Message> Messages { get; set; }
        /// <summary>
        /// This method is used when there is a GET request to Index.cshtml page.
        /// </summary>
        /// <returns>The page.</returns>
        public IActionResult OnGet()
        {
            // Get AccessToken from PageModel
            var token = AccessHelper.GetTokenFromPageModel(this);
            // If there is a token, redirect to Data
            if (token != null)
                return RedirectToPage("/Data/Get");

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
                    Logger.LogToConsole($"Messages {serializedMessages} serialization failed for user with token {token.Value}");
                    Logger.LogExceptionToConsole(e);
                }    
            }
            LoginCredentials = new LoginCredentials();
            LoginCredentials.LoginApplicationName = "";
            LoginCredentials.Username = "";
            LoginCredentials.Password = "";
            return Page();
        }
        /// <summary>
        /// OnPostAsync method is invoked after clicking on Log in button.
        /// </summary>
        /// <returns>Redirect to Data/Get page or the same page with validation messages.</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // Log in request to the server via accountService
            var response = await accountService.Login(LoginCredentials);
            var messages = new List<Message>();
            try
            {
                // If response status code if successfull, parse and save token and redirect to get page
                if (response.IsSuccessStatusCode)
                {
                    var JWTToken = response.Content.ReadAsStringAsync().Result;
                    // Save token to the session
                    HttpContext.Session.SetString(Constants.SessionJWTKey, JWTToken);
                    var token = AccessHelper.GetTokenFromPageModel(this);
                    return RedirectToPage("/Data/Get");
                }
                // Otherwise try parse error messages and display them at the create page
                else
                {
                    messages = JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync()) ?? throw new JsonSerializationException();
                }
            }
            catch (JsonSerializationException e)
            {
                // In case of JSON parsing error, create server error message
                messages.Add(MessageHepler.Create1007());
                Logger.LogExceptionToConsole(e);
            }
            Messages = messages;
            return Page();
        }
    }
}
