using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RazorWebApp;
using RazorWebApp.Repositories;
using SharedLibrary.Descriptors;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.Models;

namespace RazorWebApp.Helpers
{
    public class ControllerHelper
    {
        // backend
        DatabaseContext context;
        /// <summary>
        /// ControllerHelper constructor.
        /// </summary>
        /// <param name="databaseContext"></param>
        public ControllerHelper(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// This method finds user acccessing controller.
        /// </summary>
        /// <param name="identity">ClaimsPrincipal item recieved in controller.</param>
        /// <returns>UserModel or null if no user war found.</returns>
        public UserModel Authenticate(ClaimsIdentity identity)
        {
            // if user is authenticated and JWT contains claim named LoginApplicationName
            if (identity == null || !identity.IsAuthenticated 
                                 || identity.FindFirst("UserId") == null)
                // user is not authorized to access application descriptor for application appName
            {
                //TODO
                Logger.LogToConsole("");
                return null;
            }
            // get user id for UserId claim
            long userId;
            if (!long.TryParse(identity.FindFirst("UserId").Value, out userId))
            {
                Logger.LogToConsole($"UserId claim with value \"{identity.FindFirst("UserId").Value}\" could not be parsed.");
                return null;
            }
            // try to look for user in DB
            var userRepository = new UserRepository(context);
            var userModel = userRepository.GetById(userId);
            if (userModel == null)
            {
                Logger.LogToConsole($"No user with id \"{userId}\" found.");
                return null;
            }
            Logger.LogToConsole($"User with id \"{userModel.Id}\" successfully authenticated.");
            return userModel;
        }
        // public bool Authorize(UserModel userModel, long datasetId, RightsEnum minimalRights)
        // {
        //     return (RightsEnum)userModel.Rights.DataDictionary[datasetId] >= minimalRights;
        // }
        public Dictionary<string, List<long>> GetAllReferencesIdsDictionary(ApplicationModel applicationModel)
        {
            var stringKeyDictionary = new Dictionary<string, List<long>>();
            var dataRepository = new DataRepository(context);
            var longKeyDictionary = dataRepository.GetAllReferencesIdsDictionary(applicationModel.Id);
            foreach (var item in longKeyDictionary)
            {
                string key = applicationModel.ApplicationDescriptor.Datasets.First(d => d.Id == item.Key).Name;
                stringKeyDictionary.Add(key, item.Value);
            }
            var userRepository = new UserRepository(context);
            var longKeyUserReferences = userRepository.GetAllReferencesIdsDictionary(applicationModel.Id);
            stringKeyDictionary.Add(applicationModel.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name,
                                    longKeyUserReferences.Value);
            return stringKeyDictionary;
        }
        
        
        
        
        public bool IfCanBeDeletedPerformDeleteActions(UserModel authUserModel, IBaseModelWithApplicationAndData modelToDelete)
        {
            // // Check if modelToDelete can be referenced
            // if (!datasetIsAsReference(applicationDescriptor, modelToDelete))
            //     return true;
            // Check if modelToDelete is not already deleted or modified
            // to by byla cyklicka reference
            if (context.Entry(modelToDelete).State == EntityState.Deleted || context.Entry(modelToDelete).State == EntityState.Modified)
                return true;
            var allModelsReferencingModelToDelete = getAllReferencing(authUserModel.Application, modelToDelete); // bez duplicit
            var applicationDescriptor = authUserModel.Application.ApplicationDescriptor;

            string modelToDeleteDatasetName;
            if (modelToDelete.GetType() == typeof(DataModel))
                modelToDeleteDatasetName = applicationDescriptor.Datasets.Where(d => d.Id == ((DataModel)modelToDelete).DatasetId).FirstOrDefault().Name;
            else
                modelToDeleteDatasetName = applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name;
            foreach (var model in allModelsReferencingModelToDelete)
            {
                List<AttributeDescriptor> attributes;
                if (model.GetType() == typeof(DataModel))
                    attributes = applicationDescriptor.Datasets.Where(d => d.Id == ((DataModel)model).DatasetId).FirstOrDefault().Attributes.Where(a => a.Type == modelToDeleteDatasetName).ToList();
                else
                    attributes = applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes.Where(a => a.Type == modelToDeleteDatasetName).ToList();
                // prochazim atributy modelu, ktere jsou type model to delete name
                foreach (var attribute in attributes)
                {
                    // referenced with cascade action
                    if (attribute.OnDeleteAction == OnDeleteActionEnum.Cascade)
                    {
                        if (!this.IfCanBeDeletedPerformDeleteActions(authUserModel, model))
                            return false;
                        else
                            // remove model - bude to takhle fungovat?
                            context.Entry(model).State = EntityState.Deleted;
                    }
                    // referenced with setEmpty action
                    else if (attribute.OnDeleteAction == OnDeleteActionEnum.SetEmpty)
                    {
                        var dataDictionary = model.DataDictionary;
                        dataDictionary[attribute.Name].RemoveAll(i => i.ToString() == modelToDelete.Id.ToString());
                        model.Data = JsonConvert.SerializeObject(dataDictionary);
                    }
                    // referenced with protect action
                    else if (attribute.OnDeleteAction == OnDeleteActionEnum.Protect)
                        return false;
                }
            }
            // no pretect reference found, every set empty performed and all cascades did recursively the same
            return true;
        }
        List<IBaseModelWithApplicationAndData> getAllReferencing(ApplicationModel applicationModel, IBaseModelWithApplicationAndData modelToBeReferenced)
        {
            string referenceName;
            var applicationDescriptor = applicationModel.ApplicationDescriptor;
            var allReferences = new List<IBaseModelWithApplicationAndData>();
            if (modelToBeReferenced.GetType() == typeof(DataModel))
                referenceName = applicationDescriptor.Datasets.Where(d => d.Id == ((DataModel)modelToBeReferenced).DatasetId).FirstOrDefault().Name;
            else
                referenceName = applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name;

            // in users
            if (applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes.Any(a => a.Type == referenceName))
            {
                var userRepository = new UserRepository(context);
                allReferences.AddRange(userRepository.GetAllByApplicationIdAndDataContentLike(applicationModel.Id, modelToBeReferenced.Id.ToString()));
            }
            // in user defined datasets
            var dataRepository = new DataRepository(context);
            foreach (var datasetDescriptor in applicationDescriptor.Datasets)
            {
                if (datasetDescriptor.Attributes.Any(a => a.Type == referenceName))
                {
                    allReferences.AddRange(dataRepository.GetAllByApplicationIdAndDatasetIdAndDataContentLike(applicationModel.Id, datasetDescriptor.Id, modelToBeReferenced.Id.ToString()));
                }
            }
            return allReferences;
        }
    }
}