using System.Collections.Generic;

namespace SharedLibrary.StaticFiles
{
    public static class Constants
    {
        public static int SaltLength => 32;
        public static string ResetPasswordValue => "MetaRMS123";
        public static int MinSaferPasswordLength => 8;
        public static string SaferPasswordPattern => @"(?=^.{" + Constants.MinSaferPasswordLength + @",}$)((?=.*\d)|(?=.*\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$";
        // public static string LocalServerBaseAddress => "https://0.0.0.0:443/api/";
        public static string LocalServerBaseAddress => "http://localhost:5000/api/";
        public static string SapoiAspifyServerBaseAddress => "http://sapoi.aspifyhost.com/api/";
        public static string CacheDescriptorPrefix => "Descriptor_";
        public static string CacheRightsPrefix => "Rights_";
        public static string JWTClaimApplicationId => "ApplicationId";
        public static string JWTClaimUserId => "UserId";
        public static string MetaRMSAdministratorContactEmail => "sapoiapps@gmail.com";
        public static string SessionJWTKey => "sessionJWT";
        public static int RazorCacheTimespanInMinutes = 5;

        #region References
        /// <summary>
        /// This number defines how many attributes will be shown when creating text representation of a record.
        /// </summary>
        public static int MaxAttributesDisplayedInReference = 3;
        /// <summary>
        /// If an attribute in record contains list of references this number defines how many of them 
        /// will be in the text representation.
        /// </summary>
        public static int MaxReferencedDisplayedInListOfReferences = 3;
        public static int MaxDepthOfDisplayedReferences = 3;
        #endregion
    }
}