using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace DatabaseAccessLibrary.Operations
{
    public class FullOperations : IOperations
    {
        public DbContext _context;
        public FullOperations(DbContext context)
        {
            this._context = context;
        }
        public Task<int> Create(DataModel dataset)
        {
            var cosi = _context.Add(dataset);
            return _context.SaveChangesAsync();
        }

        public Task<int> CreateList(List<DataModel> datasets)
        {
            throw new System.NotImplementedException();
        }

        public bool DeleteById(long id)
        {
            throw new System.NotImplementedException();
        }

        public DataModel FindByAny(string searchedString)
        {
            throw new System.NotImplementedException();
        }

        public DataModel FindById(long id)
        {
            throw new System.NotImplementedException();
        }

        public bool Update(DataModel dataset)
        {
            throw new System.NotImplementedException();
        }

        public bool UpdateList(List<DataModel> datasets)
        {
            throw new System.NotImplementedException();
        }
    }
}