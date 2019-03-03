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

namespace RazorWebApp.Pages.User
{
    /// <summary>
    /// The GetModel class in RazorWebApp.Pages.Users namespace is used as support for Get.cshtml page. 
    /// The page is used to display all application users, as well as create, edit and delete action buttons.
    /// </summary>
    public class GetModel : PageModel
    {
        /// <summary>
        /// Service for user based requests to the server.
        /// </summary>
        private readonly IUserService userService;
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
        /// <param name="userService">User service to be used</param>
        /// <param name="accountService">Account service to be used</param>
        /// <param name="memoryCache">Cache to be used</param>
        public GetModel(IUserService userService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this.userService = userService;
            this.accountService = accountService;
            this.cache = memoryCache;
        }

        /// <summary>
        /// ApplicationDescriptor property contains descriptor of the signed user.
        /// </summary>
        /// <value>ApplicationDescriptor class</value>
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        /// <summary>
        /// Data property contains list of UserModel to be displayed in a data table.
        /// </summary>
        /// <value>List of UserModel</value>
        public List<UserModel> Data { get; set; }
        /// <summary>
        /// RightsRights property conatins user's rights to the rights dataset.async 
        /// This value is used for displaying Create, Edit and Delete buttons.
        /// </summary>
        /// <value>RightsEnum value.</value>
        public RightsEnum? UsersRights { get; set; }
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
        /// This method is used when there is a GET request to User/Get.cshtml page.
        /// </summary>
        /// <returns>The page.</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Index");

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
                    Messages = JsonConvert.DeserializeObject<List<Message>>((string)serializedMessages) ?? throw new JsonSerializationException();
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
            UsersRights = AccessHelper.GetRights(rights, (long)SystemDatasetsEnum.Users);
            // Menu data
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
            if (UsersRights == null || MenuData == null)
            {
                Logger.LogToConsole($"RightsRights or MenuData failed loading for user with token {token.Value}.");
                return RedirectToPage("/Errors/ServerError");
            }
            // Data
            Data = new List<UserModel>();

            #endregion

            // Authorization
            if (AuthorizationHelper.IsAuthorized(rights, (long)SystemDatasetsEnum.Users, RightsEnum.R))
            {
                // Data request to the server via userService
                var response = await userService.GetAll(token.Value);
                try
                {
                    // If response status code if successfull, try parse data
                    if (response.IsSuccessStatusCode)
                        Data = JsonConvert.DeserializeObject<List<UserModel>>(await response.Content.ReadAsStringAsync());
                    // If user is not authenticated, redirect to login page
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        return RedirectToPage("/Index");
                    // If user is not authorized, add message
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                        Messages.Add(new Message(MessageTypeEnum.Error, 
                                                 3009, 
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
                                         3009, 
                                         new List<string>()));

            return Page();
        }
        /// <summary>
        /// OnPostUserResetPasswordAsync method is invoked after clicking on Reset password button.
        /// </summary>
        /// <param name="dataId">Id of user to reset password to</param>
        /// <returns>Login page or a page with messages</returns>
        public async Task<IActionResult> OnPostUserResetPasswordAsync(long dataId)
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Index");

            // Authorization
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            // If user is not authorized to delete, add message and display page again
            if (!AuthorizationHelper.IsAuthorized(rights, (long)SystemDatasetsEnum.Users, RightsEnum.RU))
            {
                // Set messages to cookie
                TempData["Messages"] = JsonConvert.SerializeObject(
                    new List<Message>(){ new Message(MessageTypeEnum.Error, 
                                                     3010, 
                                                     new List<string>())});
                return await OnGetAsync();
            }
            
            // Reset password request to the server via userService
            var response = await userService.ResetPasswordById(dataId, token.Value);
            var messages = new List<Message>();
            try
            {
                // If response status code if successfull, parse messages
                if (response.IsSuccessStatusCode)
                    messages = JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync());
                // If user is not authenticated, redirect to login page
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Index");
                // If user is not authorized, add message
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                             3010, 
                                             new List<string>()));
                // Otherwise try parse error messages
                else
                    messages = JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync()) ?? throw new JsonSerializationException();
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
        /// OnPostUserDeleteAsync method is invoked after clicking on Delete button.
        /// </summary>
        /// <param name="dataId">Id of data to be deleted</param>
        /// <returns>Login page or a page with messages</returns>
        public async Task<IActionResult> OnPostUserDeleteAsync(long dataId)
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Index");

            // Authorization
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            // If user is not authorized to delete, add message and display page again
            if (!AuthorizationHelper.IsAuthorized(rights, (long)SystemDatasetsEnum.Users, RightsEnum.CRUD))
            {
                // Set messages to cookie
                TempData["Messages"] = JsonConvert.SerializeObject(
                    new List<Message>(){ new Message(MessageTypeEnum.Error, 
                                                     3010, 
                                                     new List<string>())});
                return await OnGetAsync();
            }
            
            // Delete request to the server via userService
            var response = await userService.DeleteById(dataId, token.Value);
            var messages = new List<Message>();
            try
            {
                // If response status code if successfull, parse messages
                if (response.IsSuccessStatusCode)
                    messages = JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync());
                // If user is not authenticated, redirect to login page
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Index");
                // If user is not authorized, add message
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                             3010, 
                                             new List<string>()));
                // Otherwise try parse error messages
                else
                    messages = JsonConvert.DeserializeObject<List<Message>>(await response.Content.ReadAsStringAsync()) ?? throw new JsonSerializationException();
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
        /// OnPostUserCreateAsync method is invoked after clicking on Create button
        /// and redirects user to create page.
        /// </summary>
        /// <returns>Redirect to create page.</returns>
        public IActionResult OnPostUserCreateAsync()
        {
            return RedirectToPage("Create", "");
        }
        /// <summary>
        /// OnPostUserEditAsync method is invoked after clicking on Edit button 
        /// and redirects user to edit page.
        /// </summary>
        /// <returns>Redirect to edit page.</returns>
        public IActionResult OnPostUserEditAsync(string dataId)
        {
            return RedirectToPage("Edit", "", new { id = dataId });
        }
    }
}
