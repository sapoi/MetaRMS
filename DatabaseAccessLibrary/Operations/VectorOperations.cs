// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using DatabaseAccessLibrary.Models;
// using Microsoft.EntityFrameworkCore;
// using SharedLibrary.Descriptors;
// using SharedLibrary.Models;

// namespace DatabaseAccessLibrary.Operations
// {
//     public class VectorOperations : IOperations
//     {
//         public DbContext _context;
//         public VectorOperations(DbContext context)
//         {
//             this._context = context;
//         }
//         public Task<int> Create(DataModel dataset)
//         {
//             DatasetDescriptor datasetDescriptor = DescriptorCache.GetDatasetDescriptor(_context, dataset.ApplicationName, dataset.DatasetName);
//             foreach (var attr in dataset.Data)
//             {
//                 switch (datasetDescriptor.Attributes.Find(a => a.Name == attr.Key).Type)
//                 {
//                     case "string":
//                         _context.Add(new VectorModel(dataset.DatasetName + "\"TadyBudeId\"" + '.' + attr.Key, 
//                                                     (string)attr.Value, null, null, null, null, null));
//                         break;
//                     case "datetime":
//                         _context.Add(new VectorModel(dataset.DatasetName + "\"TadyBudeId\"" + '.' + attr.Key, 
//                                                     null, (DateTime)attr.Value, null, null, null, null));
//                         break;
//                     case "int":
//                         _context.Add(new VectorModel(dataset.DatasetName + "\"TadyBudeId\"" + '.' + attr.Key, 
//                                                     null, null, (int)attr.Value, null, null, null));
//                         break;
//                     case "bool":
//                         _context.Add(new VectorModel(dataset.DatasetName + "\"TadyBudeId\"" + '.' + attr.Key, 
//                                                     null, null, null, (bool)attr.Value, null, null));
//                         break;
//                     case "double":
//                         _context.Add(new VectorModel(dataset.DatasetName + "\"TadyBudeId\"" + '.' + attr.Key, 
//                                                     null, null, null, null, (double)attr.Value, null));
//                         break;
//                     case "long":
//                         _context.Add(new VectorModel(dataset.DatasetName + "\"TadyBudeId\"" + '.' + attr.Key, 
//                                                     null, null, null, null, null, (long)attr.Value));
//                         break;
//                     default:
//                         throw new Exception();
//                 }
//             }
//             return _context.SaveChangesAsync();
//         }

//         public Task<int> CreateList(List<DataModel> datasets)
//         {
//             throw new System.NotImplementedException();
//         }

//         public bool DeleteById(long id)
//         {
//             throw new System.NotImplementedException();
//         }

//         public DataModel FindByAny(string searchedString)
//         {
//             throw new System.NotImplementedException();
//         }

//         public DataModel FindById(long id)
//         {
//             throw new System.NotImplementedException();
//         }

//         public bool Update(DataModel dataset)
//         {
//             throw new System.NotImplementedException();
//         }

//         public bool UpdateList(List<DataModel> datasets)
//         {
//             throw new System.NotImplementedException();
//         }
//     }
// }