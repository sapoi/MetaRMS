using System.Net.Http;
using System.Threading.Tasks;

namespace SharedLibrary.Services
{
    public interface ISecretService  
    {           
        Task<HttpResponseMessage> Get(string cookieData); 
    }
}