using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedLibrary.Models;

namespace DatabaseAccessLibrary.Operations
{
    public class JSONColumnOperations : IOperations
    {
        public DbContext _context;
        public JSONColumnOperations(DbContext context)
        {
            this._context = context;
        }
        public Task<int> Create(DataModel dataset)
        {
            DataColumnModel item = new DataColumnModel();
            //item.Name = dataset.Name;
            item.AllOtherData = JsonConvert.SerializeObject(dataset.Data);
            _context.Add(item);
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