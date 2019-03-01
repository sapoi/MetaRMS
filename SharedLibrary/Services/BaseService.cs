using System;
using System.Net.Http;

namespace SharedLibrary.Services
{
    public abstract class BaseService
    {
        protected HttpClient client;
        protected Uri baseAddress = new Uri("http://localhost:5000/api/");
        public BaseService()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }
    }
}