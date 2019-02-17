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
    /// The CreateModel class in RazorWebApp.Pages.Rights namespace is used as support for Create.cshtml page. 
    /// The page is used to create new application rights.
    /// </summary>
    public class CreateModel : PageModel
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
        public CreateModel(IRightsService rightsService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this.rightsService = rightsService;
            this.accountService = accountService;
            this.cache = memoryCache;
        }
        /// <summary>
        /// Name of the new rights.
        /// </summary>
        /// <value>string</value>
        [BindProperty]
        public string NewRightsName { get; set; }
        /// <summary>
        /// Dictionary containing rights value for each dataset.
        /// </summary>
        /// <value>Dictionary of long and RightsEnum</value>
        [BindProperty]
        public Dictionary<long, RightsEnum> NewRightsDictionary { get; set; }
        /// <summary>
        /// ApplicationDescriptor property contains descriptor of the signed user.
        /// </summary>
        /// <value>ApplicationDescriptor class</value>
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        /// <summary>
        /// MenuData property contains data necessary for _LoggedMenuPartial.
        /// </summary>
        /// <value>LoggedMenuPartialData structure</value>
        public LoggedMenuPartialData MenuData { get; set; }
        /// <summary>
        /// Messages property contains list of messages for user.
        /// </summary>
        /// <value>List of Message structure</value>
        public List<Message> Messages { get; set; }
        /// <summary>
        /// This method is used when there is a GET request to Rights/Create.cshtml page
        /// </summary>
        /// <returns>The page.</returns>
        public async Task<IActionResult> OnGetAsync()
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
            if (AccessHelper.GetRights(rights, (long)SystemDatasetsEnum.Rights) < RightsEnum.CRU)
                return RedirectToPage("/Rights/Get", new { messages = new List<Message>() {
                    new Message(MessageTypeEnum.Error, 
                                4010, 
                                new List<string>())}});

            # region PAGE DATA PREPARATION

            Messages = new List<Message>();
            // Application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(cache, accountService, token);
            if (ApplicationDescriptor == null)
            {
                Logger.LogToConsole($"Application descriptor for user with token {token.Value} not found.");
                return RedirectToPage("/Errors/ServerError");
            }
            // Menu data
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
            // NewDataDictionary - prepare keys
            NewRightsDictionary = new Dictionary<long, RightsEnum>();
            foreach (var key in ApplicationDescriptor.Datasets)
                NewRightsDictionary.Add(key.Id, 0);
            NewRightsDictionary.Add(((long)SystemDatasetsEnum.Users), 0);
            NewRightsDictionary.Add(((long)SystemDatasetsEnum.Rights), 0);
            NewRightsName = "";

            #endregion

            return Page();
        }
        /// <summary>
        /// OnPostAsync method is invoked after clicking on Submit button.
        /// </summary>
        /// <returns>Redirect to Get page or the same page with validation messages</returns>
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

            // Prepare new RightsModel
            RightsModel newRightsModel = new RightsModel(){ ApplicationId = token.ApplicationId, 
                                                            Name = NewRightsName, 
                                                            Data = JsonConvert.SerializeObject(NewRightsDictionary) };
            
            // Create request to the server via rightsService
            var response = await rightsService.Create(newRightsModel, token.Value);
            var messages = new List<Message>();
            try
            {
                // If response status code if successfull, parse messages and redirect to get page
                if (response.IsSuccessStatusCode)
                {
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
                                             4010, 
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
