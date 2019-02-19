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
using RazorWebApp.Structures;
using Microsoft.AspNetCore.Mvc.Rendering;
using SharedLibrary.Structures;
using SharedLibrary.Helpers;
using SharedLibrary.Enums;
using System.Net;

namespace RazorWebApp.Pages.Data
{
    /// <summary>
    /// The CreateModel class in RazorWebApp.Pages.Data namespace is used as support for Create.cshtml page. 
    /// The page is used to create new application data for dataset.
    /// </summary>
    public class CreateModel : PageModel
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
        /// Service for user based requests to the server.
        /// </summary>
        private readonly IUserService userService;
        /// <summary>
        /// In-memory cache service.
        /// </summary>
        private IMemoryCache cache;
        /// <summary>
        /// Constructor for initializing services and cache.
        /// </summary>
        /// <param name="dataService">Data service to be used</param>
        /// <param name="accountService">Account service to be used</param>
        /// <param name="userService">User service to be used</param>
        /// <param name="memoryCache">Cache to be used</param>
        public CreateModel(IDataService dataService, IAccountService accountService, IUserService userService, IMemoryCache memoryCache)
        {
            this.dataService = dataService;
            this.accountService = accountService;
            this.userService = userService;
            this.cache = memoryCache;
        }
        /// <summary>
        /// DatasetName property contains name of dataset the new data belongs to.
        /// </summary>
        /// <value>string containing dataset name</value>
        [BindProperty]
        public string DatasetName { get; set; }
        /// <summary>
        /// Dictionary containing string attribute name as key and list of strings as the values.
        /// </summary>
        /// <value>Dictionary of string and list of strings</value>
        [BindProperty]
        public Dictionary<string, List<string>> NewDataDictionary { get; set; }
        /// <summary>
        /// SelectData property contains data used for select html input fields.
        /// The key is attribute type and value is list of possible select values.
        /// </summary>
        /// <value>Dictionary string and list of SelectListItem</value>
        [BindProperty]
        public Dictionary<string, List<SelectListItem>> SelectData { get; set; }
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
        /// List of DatasetDescriptor with at least read rights.
        /// </summary>
        /// <value>List of DatasetDescriptor</value>
        public List<DatasetDescriptor> ReadAuthorizedDatasets { get; set; }
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
        /// This method is used when there is a GET request to Data/Create.cshtml page
        /// </summary>
        /// <returns>The page.</returns>
        public async Task<IActionResult> OnGetAsync(string datasetName)
        {
            // Authentication
            var token = AccessHelper.GetTokenFromPageModel(this);
            if (token == null)
                return RedirectToPage("/Account/Login");

            // Application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(cache, accountService, token);
            if (ApplicationDescriptor == null)
            {
                Logger.LogToConsole($"Application descriptor for user with token {token.Value} not found.");
                return RedirectToPage("/Errors/ServerError");
            }
            // Active dataset descriptor
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            if (rights == null)
            {
                Logger.LogToConsole($"Rights not found for user with token {token.Value}.");
                return RedirectToPage("/Errors/ServerError");
            }
            ActiveDatasetDescriptor = AccessHelper.GetActiveDatasetDescriptor(ApplicationDescriptor, rights, datasetName);
            if (ActiveDatasetDescriptor == null)
            {
                Logger.LogToConsole($"Active dataset descriptor for dataset {datasetName} and user with token {token.Value} not found.");
                return RedirectToPage("/Errors/ServerError");
            }

            // Authorization
            if (AccessHelper.GetRights(rights, ActiveDatasetDescriptor.Id) < RightsEnum.CRU)
                return RedirectToPage("/Data/Get", new { messages = new List<Message>() {
                    new Message(MessageTypeEnum.Error, 
                                2009, 
                                new List<string>(){datasetName})}});

            # region PAGE DATA PREPARATION

            Messages = new List<Message>();
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
            ReadAuthorizedDatasets = AccessHelper.GetReadAuthorizedDatasets(ApplicationDescriptor, rights);
            DatasetName = ActiveDatasetDescriptor.Name;
            // NewDataDictionary - prepare keys
            NewDataDictionary = new Dictionary<string, List<string>>();
            foreach (var attribute in ActiveDatasetDescriptor.Attributes)
                NewDataDictionary.Add(attribute.Name, new List<string>());
            // SelectData
            DataLoadingHelper dlh = new DataLoadingHelper();
            SelectData = await dlh.FillSelectData(ApplicationDescriptor, 
                                                    ActiveDatasetDescriptor.Attributes, 
                                                    userService, 
                                                    dataService, 
                                                    token);
            
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

            // Application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(cache, accountService, token);
            if (ApplicationDescriptor == null)
            {
                Logger.LogToConsole($"Application descriptor for user with token {token.Value} not found.");
                return RedirectToPage("/Errors/ServerError");
            }
            // Active dataset descriptor
            var rights = await AccessHelper.GetUserRights(cache, accountService, token);
            if (rights == null)
            {
                Logger.LogToConsole($"Rights not found for user with token {token.Value}.");
                return RedirectToPage("/Errors/ServerError");
            }
            ActiveDatasetDescriptor = AccessHelper.GetActiveDatasetDescriptor(ApplicationDescriptor, rights, DatasetName);
            if (ActiveDatasetDescriptor == null)
            {
                Logger.LogToConsole($"Active dataset descriptor for dataset {DatasetName} and user with token {token.Value} not found.");
                return RedirectToPage("/Errors/ServerError");
            }

            // Authorization
            if (AccessHelper.GetRights(rights, ActiveDatasetDescriptor.Id) < RightsEnum.CRU)
            {
                return RedirectToPage("/Data/Get", new { messages = new List<Message>() {
                    new Message(MessageTypeEnum.Error, 
                                2009, 
                                new List<string>(){DatasetName})}});
            }

            // Prepare new data model
            var validationHelper = new ValidationHelper();
            validationHelper.ValidateValueList(NewDataDictionary, ActiveDatasetDescriptor.Attributes);
            var newDataModel = new DataModel(){
                ApplicationId = token.ApplicationId, 
                DatasetId = ActiveDatasetDescriptor.Id,
                Data = JsonConvert.SerializeObject(NewDataDictionary)
            };

            // Create request to the server via rightsService
            var response = await dataService.Create(newDataModel, token.Value);
            var messages = new List<Message>();
            try
            {
                // If response status code if successfull, parse messages and redirect to get page
                if (response.IsSuccessStatusCode)
                {
                    // Set messages to cookie
                    TempData["Messages"] = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Data/Get");
                }
                // If user is not authenticated, redirect to login page
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Account/Login");
                // If user is not authorized, add message
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                    messages.Add(new Message(MessageTypeEnum.Error, 
                                             2009, 
                                             new List<string>(){DatasetName}));
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
            // Read authorized datasets
            ReadAuthorizedDatasets = AccessHelper.GetReadAuthorizedDatasets(ApplicationDescriptor, rights);
            // SelectData
            DataLoadingHelper dlh = new DataLoadingHelper();
            SelectData = await dlh.FillSelectData(ApplicationDescriptor, 
                                                    ActiveDatasetDescriptor.Attributes, 
                                                    userService, 
                                                    dataService, 
                                                    token);
            // Messages
            Messages = messages;

            return Page();
        }
        /// <summary>
        /// OnPostDatasetSelectAsync method is invoked after clicking on any user defined 
        /// dataset button and redirects user to page with data from that dataset.
        /// </summary>
        /// <param name="datasetName">Name of source data dataset</param>
        /// <returns>Redirect to dataset get page.</returns>
        public IActionResult OnPostDatasetSelectAsync(string datasetName)
        {
            return RedirectToPage("Get", "", new {datasetName = datasetName});
        }
    }
}
