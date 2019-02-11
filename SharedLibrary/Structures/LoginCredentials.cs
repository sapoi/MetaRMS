namespace SharedLibrary.Structures
{
    /// <summary>
    /// LoginCredentials are used on both client and server side as a container for login data.
    /// </summary>
    public class LoginCredentials 
    {
        /// <summary>
        /// LoginApplicationName property.
        /// </summary>
        /// <value>LoginApplicationName must correspond with LoginApplicationName from application descriptor.</value>
        public string LoginApplicationName { get; set; }
        /// <summary>
        /// Username property.
        /// </summary>
        /// <value>Username must be a value of attribute with Type == "username" from the application with LoginApplicationName.</value>
        public string Username { get; set; }
        /// <summary>
        /// Password property.
        /// </summary>
        /// <value>Password must correspond with password for user with Username in application with LoginApplicationName.</value>
        public string Password { get; set; }
    }
}