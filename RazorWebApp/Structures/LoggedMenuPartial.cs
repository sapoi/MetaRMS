using System.Collections.Generic;
using SharedLibrary.Enums;

namespace RazorWebApp.Structures
{
    public class LoggedMenuPartialData
    {
        public string ApplicationName;
        public string UsersDatasetName;
        public Dictionary<long, RightsEnum> NavbarRights;
    }
}