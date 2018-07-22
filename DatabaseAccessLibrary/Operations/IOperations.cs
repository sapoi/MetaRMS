using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLibrary.Models;

namespace DatabaseAccessLibrary.Operations
{
    // list of all operations provided
    interface IOperations
    {
        DataModel FindById(long id);
        DataModel FindByAny(string searchedString);

        Task<int> Create(DataModel dataset);
        Task<int> CreateList(List<DataModel> datasets);
        bool Update(DataModel dataset);
        bool UpdateList(List<DataModel> datasets);

        bool DeleteById(long id);
    }
}