using System.Collections.Generic;
using SharedLibrary.Enums;

namespace RazorWebApp.Structures
{
    public class LoggedMenuPartialData
    {
        public string AppName;
        public string UsersDatasetName;
        public Dictionary<long, RightsEnum> NavbarRights;
    }
}