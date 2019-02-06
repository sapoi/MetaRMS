namespace SharedLibrary.Structures
{
    /// <summary>
    /// LoginCredentials structure is used on both client and server side as a container for login data.
    /// </summary>
    public class LoginCredentials 
    {
        /// <summary>
        /// LoginApplicationName must correspond with LoginApplicationName from application descriptor.
        /// </summary>
        public string LoginApplicationName { get; set; }
        /// <summary>
        /// Username must be a value of attribute with Type == "username" from the application with LoginApplicationName.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Password must correspond with password for user with Username in application with LoginApplicationName.
        /// </summary>
        public string Password { get; set; }
    }
}