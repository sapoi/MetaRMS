using System;
using System.Net.Http;
using SharedLibrary.StaticFiles;

namespace SharedLibrary.Services
{
    /// <summary>
    /// Abstract BaseService class is a parent class for all services. Each service must call 
    /// BaseService constructor on initialization to initialize client and get base address.
    /// </summary>
    public abstract class BaseService
    {
        /// <summary>
        /// HttpClient instance 
        /// </summary>
        protected HttpClient client;
        #if DEBUG
            /// <summary>
            /// Local base address to connect to the server
            /// </summary>
            protected Uri baseAddress = new Uri(Constants.LocalServerBaseAddress);
        #else
            /// <summary>
            /// Production base address to connect to the server
            /// </summary>
            protected Uri baseAddress = new Uri(Constants.SapoiAspifyServerBaseAddress);
        #endif
        /// <summary>
        /// BaseService constructor for client initialization
        /// </summary>
        public BaseService()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }
    }
}