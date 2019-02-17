using SharedLibrary.Models;

namespace SharedLibrary.Structures
{
    public class JWTToken
    {
        public class AccessToken
        {
            public string Value { get; set; }
            public long UserId { get; set; }
            public long ApplicationId { get; set; }
        }
    }
}