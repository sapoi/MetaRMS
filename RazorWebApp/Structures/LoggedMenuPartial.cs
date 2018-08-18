using System.Collections.Generic;
using SharedLibrary.Enums;

namespace RazorWebApp.Structures
{
    public class LoggedMenuPartialData
    {
        public string AppName;
        public Dictionary<long, RightsEnum> NavbarRights;
    }
}