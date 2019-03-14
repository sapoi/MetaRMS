using System;
using System.Net.Http;

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
        /// <summary>
        /// Local base address to connect to the server
        /// </summary>
        protected Uri baseAddress = new Uri("http://localhost:5000/api/");
        /// <summary>
        /// Production base address to connect to the server
        /// </summary>
        // protected Uri baseAddress = new Uri("http://sapoi.aspifyhost.com/api/");
        
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