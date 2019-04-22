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
using RazorWebApp.Structures;
using Microsoft.AspNetCore.Mvc.Rendering;
using SharedLibrary.Structures;
using SharedLibrary.Helpers;
using System.Net;

namespace RazorWebApp.Pages.Account
{
    /// <summary>
    /// The SettingsModel class in Core.Pages.Account namespace is used as support for Settings.cshtml page. 
    /// The page is used to change existing password of the logged user.
    /// </summary>
    public class SettingsModel : PageModel
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
        public SettingsModel(IAccountService accountService, IMemoryCache memoryCache)
        {
            this.accountService = accountService;
            this.cache = memoryCache;
        }
        /// <summary>
        /// ApplicationDescriptor property contains descriptor of the signed user.
        /// </summary>
        /// <value>ApplicationDescriptor class</value>
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        /// MenuData property contains data necessary for _LoggedMenuPartial.
        /// </summary>
        /// <value>LoggedMenuPartialData structure</value>
        public LoggedMenuPartialData MenuData { get; set; }
        /// <summary>
        /// Structure containing old and new passwords.
        /// </summary>
        /// <value>PasswordChangeStructure structure</value>
        [BindProperty]
        public PasswordChangeStructure PasswordChangeStructure { get; set; }
        /// <summary>
        /// Messages property contains list of messages for user.
        /// </summary>
        /// <value>List of Message structure</value>
        public List<Message> Messages { get; set; }
        /// <summary>
        /// This method is used when there is a GET request to Account/Settings.cshtml page
        /// </summary>
        /// <returns>The page.</returns>
        public async Task<IActionResult> OnGetAsync(string message = null)
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Index");

            // Application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(cache, accountService, token);
            if (ApplicationDescriptor == null)
            {
                Logger.LogToConsole($"Application descriptor for user with token {token.Value} not found.");
                return RedirectToPage("/Error");
            }
            // Rights
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            if (rights == null)
            {
                Logger.LogToConsole($"Rights not found for user with token {token.Value}.");
                return RedirectToPage("/Error");
            }

            #region PAGE DATA PREPARATION

            Messages = new List<Message>();
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
            
            #endregion
        
            return Page();
        }
        /// <summary>
        /// OnPostAsync method is invoked after clicking on Change button.
        /// </summary>
        /// <returns>Redirect to the same page with validation messages</returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Index");

            // Application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(cache, accountService, token);
            if (ApplicationDescriptor == null)
            {
                Logger.LogToConsole($"Application descriptor for user with token {token.Value} not found.");
                return RedirectToPage("/Error");
            }
            // Rights
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            if (rights == null)
            {
                Logger.LogToConsole($"Rights not found for user with token {token.Value}.");
                return RedirectToPage("/Error");
            }

            #region PAGE DATA PREPARATION and INPUT VALIDATION

            Messages = new List<Message>();
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);

            // All passwords must not be null or empty strings
            if (String.IsNullOrEmpty(PasswordChangeStructure.OldPassword) ||
                String.IsNullOrEmpty(PasswordChangeStructure.NewPassword) ||
                String.IsNullOrEmpty(PasswordChangeStructure.NewPasswordCopy))
            {
                Messages.Add(new Message(MessageTypeEnum.Error, 
                                         5001, 
                                         new List<string>()));
                return Page();
            }    
            // Both new passwords must be equal
            if (PasswordChangeStructure.NewPassword != PasswordChangeStructure.NewPasswordCopy)
            {
                Messages.Add(new Message(MessageTypeEnum.Error, 
                                         5002, 
                                         new List<string>()));
                return Page();
            }

            var response = await accountService.ChangePassword(PasswordChangeStructure, token);
            try
            {
                // If user is not authenticated, redirect to login page
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Index");
                // Otherwise try parse messages and display them at the page
                else
                {
                    Messages = JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync()) ?? throw new JsonSerializationException();
                }
            }
            catch (JsonSerializationException e)
            {
                // In case of JSON parsing error, create server error message
                Messages.Add(MessageHepler.Create1007());
                Logger.LogExceptionToConsole(e);
            }

            #endregion

            return Page();
        }
    }
}
