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

namespace RazorWebApp.Pages.Rights
{
    public class CreateModel : PageModel
    {
        private readonly IRightsService _rightsService;
        private readonly IAccountService _accountService;
        private IMemoryCache _cache;

        public CreateModel(IRightsService rightsService, IAccountService accountService, IMemoryCache memoryCache)
        {
            this._rightsService = rightsService;
            this._accountService = accountService;
            this._cache = memoryCache;
        }

        public ApplicationDescriptor ApplicationDescriptor { get; set; }
        public LoggedMenuPartialData MenuData { get; set; }
        [BindProperty]
        public List<long> DatasetsIds { get; set; }
        [BindProperty]
        public Dictionary<string, int> ValueList { get; set; }
        [BindProperty]
        public string RightsName { get; set; }
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync(string message = null)
        {
            if (ModelState.IsValid)
            {
                // get token if valid
                var token = AccessHelper.ValidateAuthentication(this);
                // if token is not valid, return to login page
                if (token == null)
                    return RedirectToPage("/Account/Login");
                // get application descriptor
                ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(_cache, _accountService, token);
                if (ApplicationDescriptor == null)
                    return RedirectToPage("/Errors/ServerError");
                // get rights
                var rights = await AccessHelper.GetUserRights(_cache, _accountService, token);
                if (rights == null)
                    return RedirectToPage("/Errors/ServerError");

                MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
                
                // init ValueList
                ValueList = new Dictionary<string, int>();
                DatasetsIds = new List<long>();
                foreach (var key in ApplicationDescriptor.Datasets)
                {
                    DatasetsIds.Add(key.Id);
                    ValueList.Add(key.Id.ToString(), 0);
                }
                DatasetsIds.Add((long)SystemDatasetsEnum.Users);
                DatasetsIds.Add((long)SystemDatasetsEnum.Rights);
                ValueList.Add(((long)SystemDatasetsEnum.Users).ToString(), 0);
                ValueList.Add(((long)SystemDatasetsEnum.Rights).ToString(), 0);
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // validation
            var token = AccessHelper.ValidateAuthentication(this);
            // if token is not valid, return to login page
            if (token == null)
                return RedirectToPage("/Account/Login");
            // get application descriptor
            ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(_cache, _accountService, token);
            if (ApplicationDescriptor == null)
                return RedirectToPage("/Errors/ServerError");
            // get rights
            var rights = await AccessHelper.GetUserRights(_cache, _accountService, token);
            if (rights == null)
                return RedirectToPage("/Errors/ServerError");
            
            MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
            
            // data prepare
            // Dictionary<String, Object> inputData = new Dictionary<string, object>();
            // inputData.Add(((long)SystemDatasetsEnum.Users).ToString(), ValueList[0]);
            // inputData.Add(((long)SystemDatasetsEnum.Rights).ToString(), ValueList[1]);
            // for (int i = 2; i < DatasetsIds.Count + 2; i++)
            // {
            //     inputData.Add(DatasetsIds[i - 2].ToString(), ValueList[i]);
            // }
            
            RightsModel newRightsModel = new RightsModel() { Name = RightsName, Data = JsonConvert.SerializeObject(ValueList) };
            var response = await _rightsService.Create(ApplicationDescriptor.LoginAppName, newRightsModel, token.Value);
            string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                Message = message.Substring(1, message.Length - 2);
                return Page();
            }

            return RedirectToPage("/Rights/Get", new {message = message.Substring(1, message.Length - 2)});
        }
    }
}
