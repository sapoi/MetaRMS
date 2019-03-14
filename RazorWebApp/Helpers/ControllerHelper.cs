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
    /// <summary>
    /// ControllerHelper contains methods used by controllers on the server. 
    /// </summary>
    public class ControllerHelper
    {
        DatabaseContext context;
        /// <summary>
        /// ControllerHelper constructor.
        /// </summary>
        /// <param name="databaseContext">Database context to be used to perform database queries</param>
        public ControllerHelper(DatabaseContext context)
        {
            this.context = context;
        }
        /// <summary>
        /// This method returns user accessing controller.
        /// </summary>
        /// <param name="identity">ClaimsPrincipal item recieved in controller.</param>
        /// <returns>UserModel or null if no user was found.</returns>
        public UserModel Authenticate(ClaimsIdentity identity)
        {
            // If user is not authenticated or JWT token does not contain claim named UserId
            if (identity == null || !identity.IsAuthenticated 
                                 || identity.FindFirst("UserId") == null)
            {
                return null;
            }
            // Get user id from UserId claim
            long userId;
            if (!long.TryParse(identity.FindFirst("UserId").Value, out userId))
            {
                Logger.LogToConsole($"UserId claim with value \"{identity.FindFirst("UserId").Value}\" could not be parsed.");
                return null;
            }
            // Look for user in the database
            var userRepository = new UserRepository(context);
            var userModel = userRepository.GetById(userId);
            if (userModel == null)
            {
                Logger.LogToConsole($"No user with id \"{userId}\" found.");
                return null;
            }
            // User successfully authenticated
            Logger.LogToConsole($"User with id \"{userModel.Id}\" successfully authenticated.");
            return userModel;
        }
        /// <summary>
        /// This method returns dictionary of valid references for each dataset (including system users dataset). 
        /// This can be later used for valid references validation.
        /// </summary>
        /// <param name="applicationModel">Application to get the valid references for</param>
        /// <returns>Dictionary with dataset name as key and valid ids for the dataset as value.</returns>
        public Dictionary<string, List<long>> GetAllReferencesIdsDictionary(ApplicationModel applicationModel)
        {
            var stringKeyDictionary = new Dictionary<string, List<long>>();
            // Get data references
            var dataRepository = new DataRepository(context);
            var longKeyDictionary = dataRepository.GetAllReferencesIdsDictionary(applicationModel.Id);
            foreach (var item in longKeyDictionary)
            {
                // Get dataset name
                string key = applicationModel.ApplicationDescriptor.Datasets.First(d => d.Id == item.Key).Name;
                stringKeyDictionary.Add(key, item.Value);
            }
            // Get user references
            var userRepository = new UserRepository(context);
            var longKeyUserReferences = userRepository.GetAllReferencesIdsDictionary(applicationModel.Id);
            stringKeyDictionary.Add(applicationModel.ApplicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name,
                                    longKeyUserReferences.Value);

            return stringKeyDictionary;
        }
        /// <summary>
        /// This method checks if modelToDelete can be deleted. It finds all models where modelToDelete is
        /// referenced and based on the OnDeleteAction it recursively deletes or sets to emty those models.
        /// If the OnDeleteAction is protect, the deletion process is stopped.
        /// </summary>
        /// <param name="authUserModel">Authenticated user model</param>
        /// <param name="modelToDelete">Model to be deleted</param>
        /// <returns>
        /// True if modelToDelete can be deleted, false otherwise. Also operations on models that should be referenced
        /// or should have referenced removed are performed in the database context.
        /// </returns>
        public bool IfCanBeDeletedPerformDeleteActions(UserModel authUserModel, IBaseModelWithApplicationAndData modelToDelete)
        {
            // Check if modelToDelete is not already marked as deleted or modified
            if (context.Entry(modelToDelete).State == EntityState.Deleted || context.Entry(modelToDelete).State == EntityState.Modified)
                return true;
            // Get all models that reference modelToDelete without duplications
            var allModelsReferencingModelToDelete = getAllReferencing(authUserModel.Application, modelToDelete);
            var applicationDescriptor = authUserModel.Application.ApplicationDescriptor;
            // Get name of dataset the modelToDelete is from
            string modelToDeleteDatasetName;
            if (modelToDelete.GetType() == typeof(DataModel))
                modelToDeleteDatasetName = applicationDescriptor.Datasets.Where(d => d.Id == ((DataModel)modelToDelete).DatasetId).FirstOrDefault().Name;
            else
                modelToDeleteDatasetName = applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name;
            // Go through references
            foreach (var model in allModelsReferencingModelToDelete)
            {
                List<AttributeDescriptor> attributes;
                if (model.GetType() == typeof(DataModel))
                    attributes = applicationDescriptor.Datasets.Where(d => d.Id == ((DataModel)model).DatasetId).FirstOrDefault().Attributes.Where(a => a.Type == modelToDeleteDatasetName).ToList();
                else
                    attributes = applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes.Where(a => a.Type == modelToDeleteDatasetName).ToList();
                // Go through attributes of type modelToDelete.Name
                foreach (var attribute in attributes)
                {
                    // Referenced with cascade action
                    if (attribute.OnDeleteAction == OnDeleteActionEnum.Cascade)
                    {
                        if (!this.IfCanBeDeletedPerformDeleteActions(authUserModel, model))
                            return false;
                        else
                            context.Entry(model).State = EntityState.Deleted;
                    }
                    // Referenced with setEmpty action
                    else if (attribute.OnDeleteAction == OnDeleteActionEnum.SetEmpty)
                    {
                        var dataDictionary = model.DataDictionary;
                        dataDictionary[attribute.Name].RemoveAll(i => i.ToString() == modelToDelete.Id.ToString());
                        model.Data = JsonConvert.SerializeObject(dataDictionary);
                    }
                    // Referenced with protect action
                    else if (attribute.OnDeleteAction == OnDeleteActionEnum.Protect)
                        return false;
                }
            }
            // No protect reference found, every set empty performed and all cascades did recursively performed delete
            return true;
        }
        /// <summary>
        /// This method returns all models that use modelToBeReferenced as a reference.
        /// </summary>
        /// <param name="applicationModel">Model of authenticated user's application</param>
        /// <param name="modelToBeReferenced">Model whose references to return</param>
        /// <returns>List of models that have model from parameter as a reference.</returns>
        List<IBaseModelWithApplicationAndData> getAllReferencing(ApplicationModel applicationModel, IBaseModelWithApplicationAndData modelToBeReferenced)
        {
            var applicationDescriptor = applicationModel.ApplicationDescriptor;
            var allReferences = new List<IBaseModelWithApplicationAndData>();
            // Get reference name - this is a attribute.Type of attributes that can reference it
            string referenceName;
            if (modelToBeReferenced.GetType() == typeof(DataModel))
                referenceName = applicationDescriptor.Datasets.Where(d => d.Id == ((DataModel)modelToBeReferenced).DatasetId).FirstOrDefault().Name;
            else
                referenceName = applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Name;

            // References in system users if a attribute of type referenceName is present
            if (applicationDescriptor.SystemDatasets.UsersDatasetDescriptor.Attributes.Any(a => a.Type == referenceName))
            {
                var userRepository = new UserRepository(context);
                allReferences.AddRange(userRepository.GetAllByApplicationIdAndDataContentLike(applicationModel.Id, modelToBeReferenced.Id.ToString()));
            }
            // References in user defeined datasets
            var dataRepository = new DataRepository(context);
            foreach (var datasetDescriptor in applicationDescriptor.Datasets)
            {
                // If any of the attributes is of type referenceName
                if (datasetDescriptor.Attributes.Any(a => a.Type == referenceName))
                {
                    allReferences.AddRange(dataRepository.GetAllByApplicationIdAndDatasetIdAndDataContentLike(applicationModel.Id, datasetDescriptor.Id, modelToBeReferenced.Id.ToString()));
                }
            }

            return allReferences;
        }
    }
}