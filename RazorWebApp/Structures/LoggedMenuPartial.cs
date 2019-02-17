using System.Collections.Generic;
using SharedLibrary.Enums;

namespace RazorWebApp.Structures
{
    public class LoggedMenuPartialData
    {
        public string ApplicationName { get; set; }
        public string UsersDatasetName { get; set; }
        public Dictionary<long, RightsEnum> NavbarRights { get; set; }
    }
}