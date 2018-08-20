// using System;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using System.Collections.Generic;
// using SharedLibrary.Services;
// using SharedLibrary.Models;
// using Microsoft.AspNetCore.Http;
// using Newtonsoft.Json;
// using Microsoft.Extensions.Caching.Memory;
// using System.IdentityModel.Tokens.Jwt;
// using System.Linq;
// using SharedLibrary.Descriptors;
// using RazorWebApp.Helpers;
// using SharedLibrary.Enums;
// using RazorWebApp.Structures;
// using Microsoft.AspNetCore.Mvc.Rendering;

// namespace RazorWebApp.Pages.Account
// {
//     public class SettingsModel : PageModel
//     {
//         private readonly IAccountService _accountService;
//         private IMemoryCache _cache;

//         public SettingsModel(IAccountService accountService, IMemoryCache memoryCache)
//         {
//             this._accountService = accountService;
//             this._cache = memoryCache;
//         }

//         public ApplicationDescriptor ApplicationDescriptor { get; set; }
//         public LoggedMenuPartialData MenuData { get; set; }
//         [BindProperty]
//         public List<string> UserDataKeys { get; set; }
//         [BindProperty]
//         public List<string> ValueList { get; set; }
//         public IEnumerable<SelectListItem> UserRightsList { get; set; }
//         public string Message { get; set; }

//         public async Task<IActionResult> OnGetAsync(string message = null)
//         {
//             if (ModelState.IsValid)
//             {
//                 // get token if valid
//                 var token = AccessHelper.ValidateAuthentication(this);
//                 // if token is not valid, return to login page
//                 if (token == null)
//                     return RedirectToPage("/Account/Login");
//                 // get application descriptor
//                 ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(_cache, _accountService, token);
//                 if (ApplicationDescriptor == null)
//                     return RedirectToPage("/Errors/ServerError");
//                 // get rights
//                 var rights = await AccessHelper.GetUserRights(_cache, _accountService, token);
//                 if (rights == null)
//                     return RedirectToPage("/Errors/ServerError");

//                 MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);
//                 Message = message;
//             }
//             return Page();
//         }
//         public async Task<IActionResult> OnPostAsync(string returnUrl = null)
//         {
//             // validation
//             var token = AccessHelper.ValidateAuthentication(this);
//             // if token is not valid, return to login page
//             if (token == null)
//                 return RedirectToPage("/Account/Login");
//             // get application descriptor
//             ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(_cache, _accountService, token);
//             if (ApplicationDescriptor == null)
//                 return RedirectToPage("/Errors/ServerError");
//             // get rights
//             var rights = await AccessHelper.GetUserRights(_cache, _accountService, token);
//             if (rights == null)
//                 return RedirectToPage("/Errors/ServerError");

//             MenuData = AccessHelper.GetMenuData(ApplicationDescriptor, rights);

//             if (ValueList.Count < 3)
//                 return RedirectToPage("/Errors/ServerError");
//             if (ValueList[0] != ValueList[1])
//             {
//                 Message = "Old passwords do not match.";
//                 return Page();
//             }
//             var response = await _accountService.ChangePassword(ApplicationDescriptor.AppName, ValueList[0], ValueList[2], token.Value);
//             string message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

//             if (!response.IsSuccessStatusCode)
//             {
//                 Message = message.Substring(1, message.Length - 2);
//                 return Page();
//             }

//             return await OnGetAsync(message.Substring(1, message.Length - 2));
//         }
//     }
// }
