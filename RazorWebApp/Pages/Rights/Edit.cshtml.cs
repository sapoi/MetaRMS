using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using SharedLibrary.Services;
using SharedLibrary.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using SharedLibrary.Descriptors;
using RazorWebApp.Helpers;
using SharedLibrary.Enums;
using RazorWebApp.Structures;
using SharedLibrary.Structures;
using SharedLibrary.Helpers;
using System.Net;

namespace RazorWebApp.Pages.Rights
{
    /// <summary>
    /// The EditModel class in RazorWebApp.Pages.Rights namespace is used as support for Edit.cshtml page. 
    /// The page is used to edit existing application rights.
    /// </summary>
    public class EditModel : PageModel
    {
        /// <summary>
        /// Service for RightsModel based requests to the server.
        /// </summary>
        private readonly IRightsService rightsService;
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
        /// <param name="rightsService">Rights service to be used</param>
        /// <param name="accountService">Account service to be used</param>
        /// <param name="memoryCache">Cache to be used</param>
        public EditModel(IRightsService rightsService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this.rightsService = rightsService;
            this.accountService = accountService;
            this.cache = memoryCache;
        }

        [BindProperty]
        public long RightsId { get; set; }
        [BindProperty]
        public string RightsName { get; set; }
        [BindProperty]
        public Dictionary<long, RightsEnum> RightsDictionary { get; set; }
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        public List<Message> Messages { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Account/Login");

            // Authorization
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            if (rights == null)
            {
                Logger.LogToConsole($"Rights not found for user with token {token.Value}.");
                return RedirectToPage("/Errors/ServerError");
            }
            if (AccessHelper.GetRights(rights, (long)SystemDatasetsEnum.Rights) < RightsEnum.RU)
                return RedirectToPage("/Rights/Get", new { messages = new List<Message>() {
                    new Message(MessageTypeEnum.Error, 
                                4011, 
                                new List<string>())}});

            # region PAGE DATA PREPARATION

            Messages = new List<Message>();
            RightsId = 0;
            RightsName = "";
            RightsDictionary = new Dictionary<long, RightsEnum>();
            // Application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(cache, accountService, token);
            if (ApplicationDescriptor == null)
            {
                Logger.LogToConsole($"Application descriptor for user with token {token.Value} not found.");
                return RedirectToPage("/Errors/ServerError");
            }
            // Menu data
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
            // Data request to the server via rightsService
            RightsModel rightsModel;
            var response = await rightsService.GetById(id, token.Value);
            try
            {
                // If response status code if successfull, try parse data
                if (response.IsSuccessStatusCode)
                {
                    rightsModel = JsonConvert.DeserializeObject<RightsModel>(await response.Content.ReadAsStringAsync());
                    // Data dictionary and data id
                    RightsId = rightsModel.Id;
                    RightsName = rightsModel.Name;
                    RightsDictionary = rightsModel.DataDictionary;
                }
                // If user is not authenticated, redirect to login page
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Account/Login");
                // If user is not authorized, add message
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                    Messages.Add(new Message(MessageTypeEnum.Error, 
                                                4011, 
                                                new List<string>()));
                // Otherwise try parse error messages
                else
                    Messages.AddRange(JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync()));
            }
            catch (JsonSerializationException e)
            {
                // In case of JSON parsing error, create server error message
                Messages.Add(MessageHepler.Create1008());
                Logger.LogExceptionToConsole(e);
            } 
            
            #endregion

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Account/Login");

            // Authorization
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            // If user is not authorized to create, add message and redirect to get page
            if (AccessHelper.GetRights(rights, (long)SystemDatasetsEnum.Rights) < RightsEnum.CRUD)
            {
                return RedirectToPage("/Rights/Get", new { messages = new List<Message>() {
                    new Message(MessageTypeEnum.Error, 
                                4010, 
                                new List<string>())}});
            }

            // Prepare edited RightsModel
            RightsModel patchedRightsModel = new RightsModel(){ ApplicationId = token.ApplicationId, 
                                                                Id = RightsId,
                                                                Name = RightsName, 
                                                                Data = JsonConvert.SerializeObject(RightsDictionary) };

            // Patch request to the server via rightsService
            var response = await rightsService.Patch(patchedRightsModel, token.Value);
            var messages = new List<Message>();
            try
            {
                // If response status code if successfull, parse messages and redirect to get page
                if (response.IsSuccessStatusCode)
                {
                    // Delete old version of rights from cache
                    CacheAccessHelper.RemoveRightsFromCache(cache);
                    // Set messages to cookie
                    TempData["Messages"] = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Rights/Get");
                }
                // If user is not authenticated, redirect to login page
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Account/Login");
                // If user is not authorized, add message
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                             4011, 
                                             new List<string>()));
                // Otherwise try parse error messages and display them at the create page
                else
                {
                    messages = JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync());
                }
            }
            catch (JsonSerializationException e)
            {
                // In case of JSON parsing error, create server error message
                messages.Add(MessageHepler.Create1008());
                Logger.LogExceptionToConsole(e);
            }

            // Application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(cache, accountService, token);
            if (ApplicationDescriptor == null)
                return RedirectToPage("/Errors/ServerError");
            // Menu data
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
            // Messages
            Messages = messages;

            return Page();
        }
    }
}
