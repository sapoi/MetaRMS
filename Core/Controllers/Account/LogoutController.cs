using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers.Account
{
    [Route("api/account/[controller]")]
    public class LogoutController : Controller
    {
        /// <summary>
        /// Database context for repository.
        /// </summary>
        private readonly DatabaseContext context;
        public LogoutController(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// API endpoint for user logout.
        /// </summary>
        /// <returns>Error of info message about action result</returns>
        /// <response code="200">If user was successfully logged out</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult Logout()
        {
            //TODO 
            // Have database of no longer active tokens that still have some time to live
            // Query provided token against this database on every authorized request
            return Ok();
        }
    }
}