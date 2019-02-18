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
    /// The GetModel class in RazorWebApp.Pages.Rights namespace is used as support for Get.cshtml page. 
    /// The page is used to display all application rights, as well as create, edit and delete action buttons.
    /// </summary>
    public class GetModel : PageModel
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
        public GetModel(IRightsService rightsService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this.rightsService = rightsService;
            this.accountService = accountService;
            this.cache = memoryCache;
        }

        /// <summary>
        /// ApplicationDescriptor property contains descriptor of the signed user.
        /// </summary>
        /// <value>ApplicationDescriptor class</value>
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        /// <summary>
        /// Data property contains list of RightsModel to be displayed in a data table.
        /// </summary>
        /// <value>List of RightsModel</value>
        public List<RightsModel> Data { get; set; }
        /// <summary>
        /// RightsRights property conatins user's rights to the rights dataset.async 
        /// This value is used for displaying Create, Edit and Delete buttons.
        /// </summary>
        /// <value>RightsEnum value.</value>
        public RightsEnum? RightsRights { get; set; }
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
        /// This method is used when there is a GET request to Rights/Get.cshtml page.
        /// </summary>
        /// <returns>The page.</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Account/Login");

            # region PAGE DATA PREPARATION

            // Messages
            Messages = new List<Message>();
            // Get messages from cookie
            var serializedMessages = TempData["Messages"];
            TempData.Remove("Messages");
            if (serializedMessages != null)
            {
                try
                {
                    Messages = JsonConvert.DeserializeObject<List<Message>>((string)serializedMessages);
                }
                catch (JsonSerializationException e)
                {
                    Logger.LogToConsole($"Messages {serializedMessages} serialization failed for user with token {token.Value}");
                    Logger.LogExceptionToConsole(e);
                }    
            }   
            // Application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(cache, accountService, token);
            if (ApplicationDescriptor == null)
            {
                Logger.LogToConsole($"Application descriptor for user with token {token.Value} not found.");
                return RedirectToPage("/Errors/ServerError");
            }
            // Rights
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            RightsRights = AccessHelper.GetRights(rights, (long)SystemDatasetsEnum.Rights);
            // Menu data
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
            if (RightsRights == null || MenuData == null)
            {
                Logger.LogToConsole($"RightsRights or MenuData failed loading for user with token {token.Value}.");
                return RedirectToPage("/Errors/ServerError");
            }
            // Data
            Data = new List<RightsModel>();

            #endregion

            // Authorization
            if (RightsRights >= RightsEnum.R)
            {
                // Data request to the server via rightsService
                var response = await rightsService.GetAll(token.Value);
                try
                {
                    // If response status code if successfull, try parse data
                    if (response.IsSuccessStatusCode)
                        Data = JsonConvert.DeserializeObject<List<RightsModel>>(await response.Content.ReadAsStringAsync());
                    // If user is not authenticated, redirect to login page
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        return RedirectToPage("/Account/Login");
                    // If user is not authorized, add message
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                        Messages.Add(new Message(MessageTypeEnum.Error, 
                                                 4008, 
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
            }
            // If user not authorized add general unauthorized message to Messages
            else
                Messages.Add(new Message(MessageTypeEnum.Error, 
                                         4008, 
                                         new List<string>()));

            return Page();
        }
        /// <summary>
        /// OnPostRightsDeleteAsync method is invoked after clicking on Delete button.
        /// </summary>
        /// <param name="dataId">Id of data to be deleted</param>
        /// <returns>Login page or a page with messages</returns>
        public async Task<IActionResult> OnPostRightsDeleteAsync(long dataId)
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Account/Login");

            // Authorization
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            // If user is not authorized to delete, add message and display page again
            if (AccessHelper.GetRights(rights, (long)SystemDatasetsEnum.Rights) < RightsEnum.CRUD)
            {
                // Set messages to cookie
                TempData["Messages"] = JsonConvert.SerializeObject(
                    new List<Message>(){ new Message(MessageTypeEnum.Error, 
                                                     4009, 
                                                     new List<string>())});
                return await OnGetAsync();
            }
            
            // Delete request to the server via rightsService
            var response = await rightsService.DeleteById(dataId, token.Value);
            var messages = new List<Message>();
            try
            {
                // If response status code if successfull, parse messages
                if (response.IsSuccessStatusCode)
                    messages = JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync());
                // If user is not authenticated, redirect to login page
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Account/Login");
                // If user is not authorized, add message
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                             4008, 
                                             new List<string>()));
                // Otherwise try parse error messages
                else
                    messages = JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync());
            }
            catch (JsonSerializationException e)
            {
                // In case of JSON parsing error, create server error message
                messages.Add(MessageHepler.Create1008());
                Logger.LogExceptionToConsole(e);
            }
            
            // Set messages to cookie
            TempData["Messages"] = JsonConvert.SerializeObject(messages);
            return await OnGetAsync();
        }
        /// <summary>
        /// OnPostRightsCreateAsync method is invoked after clicking on Create button
        /// and redirects user to create page.
        /// </summary>
        /// <returns>Redirect to create page.</returns>
        public IActionResult OnPostRightsCreateAsync()
        {
            return RedirectToPage("Create", "");
        }
        /// <summary>
        /// OnPostRightsEditAsync method is invoked after clicking on Edit button 
        /// and redirects user to edit page.
        /// </summary>
        /// <returns>Redirect to edit page.</returns>
        public IActionResult OnPostRightsEditAsync(string dataId)
        {
            return RedirectToPage("Edit", "", new { id = dataId });
        }
    }
}
