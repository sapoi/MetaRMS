using SharedLibrary.Models;

namespace SharedLibrary.Structures
{
    /// <summary>
    /// JWTToken is used on client side for storing and working with JWT token.
    /// </summary>
    public class JWTToken
    {
        /// <summary>
        /// Value proptery.
        /// </summary>
        /// <value>Original string value of the token receiver from the server.</value>
        public string Value { get; set; }
        /// <summary>
        /// UserId property
        /// </summary>
        /// <value>Long value of user id from the token.</value>
        public long UserId { get; set; }
        /// <summary>
        /// ApplicationId property.
        /// </summary>
        /// <value>Long value of application id from the token.</value>
        public long ApplicationId { get; set; }
    }
}