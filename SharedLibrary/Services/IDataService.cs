using System.Net.Http;
using System.Threading.Tasks;
using SharedLibrary.Models;
using SharedLibrary.Structures;

namespace SharedLibrary.Services
{
    /// <summary>
    /// IDataServece is a interface for services regarding to DataModel.
    /// Additional comments of each method can be found with the implementation.
    /// </summary>
    public interface IDataService  
    {
        Task<HttpResponseMessage> GetAll(long datasetId, JWTToken token);
        Task<HttpResponseMessage> GetById(long datasetId, long dataId, JWTToken token);
        Task<HttpResponseMessage> DeleteById(long datasetId, long dataId, JWTToken token);
        Task<HttpResponseMessage> Put(DataModel dataModel, JWTToken token);
        Task<HttpResponseMessage> Create(DataModel dataModel, JWTToken token);
        

    }
}