using System.Net.Http;
using System.Threading.Tasks;
using SharedLibrary.Models;

namespace SharedLibrary.Services
{
    /// <summary>
    /// IDataServece is a interface for services regarding to DataModel.
    /// Additional comments of each method can be found with the implementation.
    /// </summary>
    public interface IDataService  
    {
        Task<HttpResponseMessage> GetAll(long datasetId, string token);
        Task<HttpResponseMessage> GetById(long datasetId, long id, string token);
        Task<HttpResponseMessage> DeleteById(long datasetId, long id, string token);
        Task<HttpResponseMessage> Patch(DataModel dataModel, string token);
        Task<HttpResponseMessage> Create(DataModel dataModel, string token);
        

    }
}