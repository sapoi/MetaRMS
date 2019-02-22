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
// using SharedLibrary.Structures;

// namespace RazorWebApp.Pages.Account
// {
//     public class DefaultPasswordChangeModel : PageModel
//     {
//         private readonly IAccountService _accountService;
//         private IMemoryCache _cache;

//         public DefaultPasswordChangeModel(IAccountService accountService, IMemoryCache memoryCache)
//         {
//             this._accountService = accountService;
//             this._cache = memoryCache;
//         }

//         public ApplicationDescriptor ApplicationDescriptor { get; set; }
//         public LoggedMenuPartialData MenuData { get; set; }
//         [BindProperty]
//         public List<string> ValueList { get; set; }
//         public string Message { get; set; }

//         public async Task<IActionResult> OnGetAsync(string message = null)
//         {
//             if (ModelState.IsValid)
//             {
//                 // get token if valid
//                 var token = AccessHelper.ValidateAuthentication(this);
//                 // if token is not valid, return to login page
//                 if (token == null)
//                     return RedirectToPage("/Index");
//                 // get application descriptor
//                 ApplicationDescriptor = await AccessHelper.GetApplicationDescriptor(_cache, _accountService, token);
//                 if (ApplicationDescriptor == null)
//                     return RedirectToPage("/Errors/ServerError");
//                 // create none rights for user vithout chenged password
//                 var rights = new Dictionary<long, RightsEnum>();
//                 rights.Add(-1, RightsEnum.None);
//                 rights.Add(-2, RightsEnum.None);

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
//                 return RedirectToPage("/Index");
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
//             if (ValueList[1] != ValueList[2])
//             {
//                 Message = "New passwords do not match.";
//                 return Page();
//             }
//             var response = await _accountService.ChangePassword(ApplicationDescriptor.LoginAppName, 
//                                                                 new PasswordChange { OldPassword = ValueList[0], NewPassword = ValueList [2] }, 
//                                                                 token.Value);
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
