using System.Collections.Generic;
using SharedLibrary.Enums;

namespace RazorWebApp.Structures
{
    /// <summary>
    /// LoggedMenuPartialData class contains properties necessary for _LoggedMenuPartial.cshtml file
    /// to display menu for logged user
    /// </summary>
    public class LoggedMenuPartialData
    {
        /// <summary>
        /// Name of application the user is logged into.
        /// </summary>
        /// <value>string</value>
        public string ApplicationName { get; set; }
        /// <summary>
        /// Name of the user dataset defined in the application descriptor.
        /// </summary>
        /// <value>string</value>
        public string UsersDatasetName { get; set; }
        /// <summary>
        /// Rights for users and rights.
        /// </summary>
        /// <value>Dictionary of long and RightsEnum</value>
        public Dictionary<long, RightsEnum> NavbarRights { get; set; }
    }
}