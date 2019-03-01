using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using SharedLibrary.Services;
using SharedLibrary.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using SharedLibrary.Descriptors;
using RazorWebApp.Helpers;
using RazorWebApp.Structures;
using Microsoft.AspNetCore.Mvc.Rendering;
using SharedLibrary.Structures;
using System.Net;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using System;

namespace RazorWebApp.Pages.User
{
    /// <summary>
    /// The EditModel class in RazorWebApp.Pages.User namespace is used as support for Edit.cshtml page. 
    /// The page is used to edit existing application user.
    /// </summary>
    public class EditModel : PageModel
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
        /// Service for RightsModel based requests to the server.
        /// </summary>
        private readonly IRightsService rightsService;
        /// <summary>
        /// Service for DataModel based requests to the server.
        /// </summary>
        private readonly IDataService dataService;
        /// <summary>
        /// In-memory cache service.
        /// </summary>
        private IMemoryCache cache;
        /// <summary>
        /// Constructor for initializing services and cache.
        /// </summary>
        /// <param name="userService">User service to be used</param>
        /// <param name="accountService">Account service to be used</param>
        /// <param name="rightsService">Rights service to be used</param>
        /// <param name="dataService">Data service to be used</param>
        /// <param name="memoryCache">Cache to be used</param>
        public EditModel(IUserService userService, IAccountService accountService, IRightsService rightsService, IDataService dataService, IMemoryCache memoryCache)
        {
            this.userService = userService;
            this.accountService = accountService;
            this.rightsService = rightsService;
            this.dataService = dataService;
            this.cache = memoryCache;
        }
        /// <summary>
        /// Id of the user to edit.
        /// </summary>
        /// <value>long</value>
        [BindProperty]
        public long UserId { get; set; }
        /// <summary>
        /// Id of rights for the new created user
        /// </summary>
        /// <value>Long number</value>
        [BindProperty]
        public long UserRightsId { get; set; }
        /// <summary>
        /// Dictionary containing string attribute name as key and list of strings as the values.
        /// </summary>
        /// <value>Dictionary of string and list of strings</value>
        [BindProperty]
        public Dictionary<string, List<string>> UserDataDictionary { get; set; }
        /// <summary>
        /// SelectData property contains data used for select html input fields.
        /// The key is attribute type and value is list of possible select values.
        /// </summary>
        /// <value>Dictionary string and list of SelectListItem</value>
        public Dictionary<string, List<SelectListItem>> SelectData { get; set; }
        /// <summary>
        /// ApplicationDescriptor property contains descriptor of the signed user.
        /// </summary>
        /// <value>ApplicationDescriptor class</value>
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        /// <summary>
        /// Enumerable of application's available user rights.
        /// </summary>
        /// <value>IEnumerable of SelectListItem</value>
        public IEnumerable<SelectListItem> UserRightsData { get; set; }
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
        /// This method is used when there is a GET request to User/Edit.cshtml page
        /// </summary>
        /// <returns>The page.</returns>
        public async Task<IActionResult> OnGetAsync(long id)
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Index");

            // Authorization
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            if (rights == null)
            {
                Logger.LogToConsole($"Rights not found for user with token {token.Value}.");
                return RedirectToPage("/Errors/ServerError");
            }
            if (!AuthorizationHelper.IsAuthorized(rights, (long)SystemDatasetsEnum.Users, RightsEnum.RU))
                return RedirectToPage("/User/Get", new { messages = new List<Message>() {
                    new Message(MessageTypeEnum.Error, 
                                3012, 
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

            // Data request to the server via userService
            UserModel userModel;
            var response = await userService.GetById(id, token.Value);
            try
            {
                // If response status code if successfull, try parse data
                if (response.IsSuccessStatusCode)
                {
                    userModel = JsonConvert.DeserializeObject<UserModel>(await response.Content.ReadAsStringAsync());
                    // Data dictionary and data id
                    UserId = userModel.Id;
                    UserRightsId = userModel.RightsId;
                    // Convert Dictionary<string, List<object>> from dataModel to Dictionary<string, List<string>> expected by html page
                    UserDataDictionary = userModel.DataDictionary.ToDictionary(k => k.Key, k => k.Value.ConvertAll(x => Convert.ToString(x)));
                }
                // If user is not authenticated, redirect to login page
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Index");
                // If user is not authorized, add message
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                    Messages.Add(new Message(MessageTypeEnum.Error, 
                                                3012, 
                                                new List<string>()));
                // Otherwise try parse error messages and display them at the get page
                else
                {
                    // Set messages to cookie
                    TempData["Messages"] = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/User/Get");
                }
            }
            catch (JsonSerializationException e)
            {
                // In case of JSON parsing error, create server error message
                Messages.Add(MessageHepler.Create1008());
                Logger.LogExceptionToConsole(e);
            } 
            // SelectData
            DataLoadingHelper dlh = new DataLoadingHelper();
            SelectData = await dlh.FillSelectData(ApplicationDescriptor, 
                                                    ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes, 
                                                    userService, 
                                                    dataService, 
                                                    token);
            // UserRightsList
            UserRightsData = await dlh.FillUserRightsData(rightsService, token);
            
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
                return RedirectToPage("/Index");

            // Authorization
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            // If user is not authorized to create, add message and redirect to get page
            if (!AuthorizationHelper.IsAuthorized(rights, (long)SystemDatasetsEnum.Users, RightsEnum.RU))
            {
                return RedirectToPage("/User/Get", new { messages = new List<Message>() {
                    new Message(MessageTypeEnum.Error, 
                                3012, 
                                new List<string>())}});
            }

            // Validate and prepare UserModel
            // Application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(cache, accountService, token);
            if (ApplicationDescriptor == null)
            {
                Logger.LogToConsole($"Application descriptor for user with token {token.Value} not found.");
                return RedirectToPage("/Errors/ServerError");
            }
            var validationHelper = new ValidationHelper();
            validationHelper.ValidateValueList(UserDataDictionary, ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes);
            UserModel patchedUserModel = new UserModel() { 
                Id = UserId,
                ApplicationId = token.ApplicationId, 
                RightsId = UserRightsId,
                Data = JsonConvert.SerializeObject(UserDataDictionary) 
            };

            // Create request to the server via userService
            var response = await userService.Patch(patchedUserModel, token.Value);
            var messages = new List<Message>();
            try
            {
                // If response status code if successfull, parse messages and redirect to get page
                if (response.IsSuccessStatusCode)
                {
                    // Set messages to cookie
                    TempData["Messages"] = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/User/Get");
                }
                // If user is not authenticated, redirect to login page
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Index");
                // If user is not authorized, add message
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                             3012, 
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
            // Menu data
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
            // SelectData
            DataLoadingHelper dlh = new DataLoadingHelper();
            SelectData = await dlh.FillSelectData(ApplicationDescriptor, 
                                                    ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes, 
                                                    userService, 
                                                    dataService, 
                                                    token);
            // UserRightsList
            UserRightsData = await dlh.FillUserRightsData(rightsService, token);
            // Messages
            Messages = messages;

            return Page();
        }
    }
}
