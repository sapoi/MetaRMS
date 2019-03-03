using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using SharedLibrary.Services;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using SharedLibrary.Descriptors;
using RazorWebApp.Helpers;
using SharedLibrary.Enums;
using RazorWebApp.Structures;
using SharedLibrary.Models;
using SharedLibrary.Structures;
using SharedLibrary.Helpers;
using System.Net;
using System.Linq;

namespace RazorWebApp.Pages.Data
{
    /// <summary>
    /// The GetModel class in RazorWebApp.Pages.Data namespace is used as support for Get.cshtml page. 
    /// The page is used to display all application data for user defined datasets, as well as create, 
    /// edit and delete action buttons.
    /// </summary>
    public class GetModel : PageModel
    {
        /// <summary>
        /// Service for DataModel based requests to the server.
        /// </summary>
        private readonly IDataService dataService;
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
        /// <param name="dataService">Data service to be used</param>
        /// <param name="accountService">Account service to be used</param>
        /// <param name="memoryCache">Cache to be used</param>
        public GetModel(IDataService dataService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this.dataService = dataService;
            this.accountService = accountService;
            this.cache = memoryCache;
        }
        /// <summary>
        /// ApplicationDescriptor property contains descriptor of the signed user.
        /// </summary>
        /// <value>ApplicationDescriptor class</value>
        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        /// <summary>
        /// ActiveDatasetDescriptor property contains descriptor of currently selected dataset.
        /// </summary>
        /// <value>DatasetDescriptor class</value>
        public DatasetDescriptor ActiveDatasetDescriptor { get; set; }
        /// <summary>
        /// Data property contains list of DataModel to be displayed in a data table.
        /// </summary>
        /// <value>List of DataModel</value>
        public List<DataModel> Data { get; set; }
        // /// <summary>
        // /// List of DatasetDescriptor with at least read rights.
        // /// </summary>
        // /// <value>List of DatasetDescriptor</value>
        // public List<DatasetDescriptor> ReadAuthorizedDatasets { get; set; }
        /// <summary>
        /// ActiveDatasetRights property conatins user's rights to the active dataset. 
        /// This value is used for displaying Create, Edit and Delete buttons.
        /// </summary>
        /// <value>RightsEnum value.</value>
        public RightsEnum? ActiveDatasetRights { get; set; }
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
        /// This method is used when there is a GET request to Data/Get.cshtml page.
        /// </summary>
        /// <param name="datasetName">Name of dataset to display data from</param>
        /// <returns>The page.</returns>
        public async Task<IActionResult> OnGetAsync(string datasetName = null)
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
            // Menu data
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
            // Read authorized datasets
            // ReadAuthorizedDatasets = AccessHelper.GetReadAuthorizedDatasets(ApplicationDescriptor, rights);
            // If user has no read authorized datasets, create dummy dataset and info message.
            if (MenuData.ReadAuthorizedDatasets.Count == 0)
            {
                ActiveDatasetDescriptor = new DatasetDescriptor { Name = "", Id = 0, Attributes = new List<AttributeDescriptor>() };
                Data = new List<DataModel>();
                ActiveDatasetRights = RightsEnum.None;
                Messages.Add(new Message(MessageTypeEnum.Info, 
                                         2011, 
                                         new List<string>(){ActiveDatasetDescriptor.Name}));
                return Page();
            }
            // Active dataset descriptor
            ActiveDatasetDescriptor = AccessHelper.GetActiveDatasetDescriptor(ApplicationDescriptor, rights, datasetName);
            // And its rights
            ActiveDatasetRights = AccessHelper.GetRights(rights, ActiveDatasetDescriptor.Id);
            if (ActiveDatasetDescriptor == null || ActiveDatasetRights == null || MenuData == null)
            {
                Logger.LogToConsole($"ReadAuthorizedDatasets, ActiveDatasetDescriptor, ActiveDatasetRights or MenuData failed loading for user with token {token.Value}.");
                return RedirectToPage("/Errors/ServerError");
            }
            // Data
            Data = new List<DataModel>();

            #endregion

            // Authorization
            if (AuthorizationHelper.IsAuthorized(rights, ActiveDatasetDescriptor.Id, RightsEnum.R))
            {
                // Data request to the server via dataService
                var response = await dataService.GetAll(ActiveDatasetDescriptor.Id, token.Value);
                try
                {
                    // If response status code if successfull, try parse data
                    if (response.IsSuccessStatusCode)
                        Data = JsonConvert.DeserializeObject<List<DataModel>>(await response.Content.ReadAsStringAsync());
                    // If user is not authenticated, redirect to login page
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        return RedirectToPage("/Index");
                    // If user is not authorized, add message
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                        Messages.Add(new Message(MessageTypeEnum.Error, 
                                                 2007, 
                                                 new List<string>(){ActiveDatasetDescriptor.Name}));
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
                                         2007, 
                                         new List<string>(){ActiveDatasetDescriptor.Name}));

            return Page();
        }
        /// <summary>
        /// OnPostDataEditAsync method is invoked after clicking on Edit button
        /// and redirects user to edit page.
        /// </summary>
        /// <param name="datasetName">Name of dataset to edit the data from</param>
        /// <param name="dataId">Id of data to edit</param>
        /// <returns>Redirect to edit page.</returns>
        public IActionResult OnPostDataEditAsync(string datasetName, string dataId)
        {
            return RedirectToPage("Edit", "", new { datasetName = datasetName, id = dataId });
        }
        /// <summary>
        /// OnPostDataDeleteAsync method is invoked after clicking on Delete button.
        /// </summary>
        /// <param name="datasetName">Name of dataset that the data are from</param>
        /// <param name="dataId">Id of data to be deleted</param>
        /// <returns>Login page or a page with messages</returns>
        public async Task<IActionResult> OnPostDataDeleteAsync(string datasetName, long dataId)
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Index");

            // Authorization
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(cache, accountService, token);
            if (ApplicationDescriptor == null)
            {
                Logger.LogToConsole($"Application descriptor for user with token {token.Value} not found.");
                return RedirectToPage("/Errors/ServerError");
            }
            ActiveDatasetDescriptor = ApplicationDescriptor.Datasets.FirstOrDefault(d => d.Name == datasetName);
            if (ActiveDatasetDescriptor == null)
            {
                return await OnGetAsync(datasetName);
            }
            // If user is not authorized to delete, add message and display page again
            if (!AuthorizationHelper.IsAuthorized(rights, ActiveDatasetDescriptor.Id, RightsEnum.CRUD))
            {
                // Set messages to cookie
                TempData["Messages"] = JsonConvert.SerializeObject(
                    new List<Message>(){ new Message(MessageTypeEnum.Error, 
                                                     2008, 
                                                     new List<string>(){datasetName})});
                return await OnGetAsync();
            }

            // Delete request to the server via rightsService
            var response = await dataService.DeleteById(ActiveDatasetDescriptor.Id, dataId, token.Value);
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
                                             2008, 
                                             new List<string>(){datasetName}));
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
            return await OnGetAsync(datasetName);
        }
        /// <summary>
        /// OnPostDataCreateAsync method is invoked after clicking on Create button
        /// and redirects user to create page.
        /// </summary>
        /// <param name="datasetName">Name of dataset to create data to</param>
        /// <returns>Redirect to create page.</returns>
        public IActionResult OnPostDataCreateAsync(string datasetName)
        {
            return RedirectToPage("Create", "", new { datasetName = datasetName });
        }
    }
}
