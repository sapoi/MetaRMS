using System.Collections.Generic;

namespace SharedLibrary.StaticFiles
{
    public static class Constants
    {
        public static int SaltLength => 32;
        public static string ResetPasswordValue => "MetaRMS123";
        public static int MinSaferPasswordLength => 8;
        public static string SaferPasswordPattern => @"(?=^.{" + Constants.MinSaferPasswordLength + @",}$)((?=.*\d)|(?=.*\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$";
        public static string LocalServerBaseAddress => "http://localhost:5000/api/";
        public static string SapoiAspifyServerBaseAddress => "http://sapoi.aspifyhost.com/api/";
        public static string CacheDescriptorPrefix => "Descriptor_";
        public static string CacheRightsPrefix => "Rights_";
        public static string JWTClaimApplicationId => "ApplicationId";
        public static string JWTClaimUserId => "UserId";
        public static string MetaRMAAdministratorContactEmail => "sapoiapps@gmail.com";
        public static string SessionJWTKey => "sessionJWT";
    }
}