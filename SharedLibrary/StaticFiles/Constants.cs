using System.Collections.Generic;

namespace SharedLibrary.StaticFiles
{
    public static class Constants
    {
        #region Base addresses
        /// <summary>
        /// Base address of API used with debug configuration on HTTP or HTTPS based on Constants.UseHttp value
        /// </summary>
        public static string DebugServerBaseAddress => UseHttps ? "https://localhost:443/api/" : "http://localhost:80/api/";
        /// <summary>
        /// Base address of API used with release configuration.
        /// </summary>
        public static string ReleaseServerBaseAddress => "http://sapoi.aspifyhost.com/api/";
        #endregion

        #region Security settings
        /// <summary>
        /// Boolean indicator in the HTTPS connection is used.
        /// With debug configuration the usage of HTTPS protocol depends on the settings
        /// of Constants.UseHttps variable, but with release configuration HTTPS protocol is disabled
        /// </summary>
        public static bool UseHttps => false;
        /// <summary>
        /// Path to .pfx certificate used for HTTPS connection.
        /// </summary>
        public static string HttpsCertificatePath => "/localhost.pfx";
        /// <summary>
        /// Passwork to the certificate from variable HttpsCertificatePath
        /// </summary>
        public static string HttpsCertificatePassword => "certificate_password";
        /// <summary>
        /// Length of salt hashed together with a password.
        /// </summary>
        public static int SaltLength => 32;
        /// <summary>
        /// Default value to which a password is set when resetting it.
        /// </summary>
        public static string ResetPasswordValue => "MetaRMS123";
        /// <summary>
        /// Minimum required length of a password when Safer property of the Password apptibute is set to true.
        /// </summary>
        public static int MinSaferPasswordLength => 8;
        /// <summary>
        /// Pattern against which is a password compared when Safer property of the Password apptibute is set to true.
        /// </summary>
        public static string SaferPasswordPattern => @"(?=^.{" + Constants.MinSaferPasswordLength + @",}$)((?=.*\d)|(?=.*\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$";
        #endregion

        #region Application initialization email settings
        /// <summary>
        /// Email to send the admin login credentials from.
        /// </summary>
        public static string AppInitEmail = "sapoiapps@gmail.com";
        /// <summary>
        /// Parrwird to the email.
        /// </summary>
        public static string AppInitEmailPassword = "sapoisapoi";
        /// <summary>
        /// Host of the email.
        /// </summary>
        public static string AppInitEmailHost = "smtp.gmail.com";
        /// <summary>
        /// Email port.
        /// </summary>
        public static int AppInitEmailPort = 587;
        /// <summary>
        /// Indicatior if SSL is enabled.
        /// </summary>
        public static bool AppInitEmailEnableSsl = true;
        #endregion

        #region Web application settings
        /// <summary>
        /// Contact email to MetaRMS administartor. This contact is displayed on the error page.
        /// </summary>
        public static string MetaRMSAdministratorContactEmail => "sapoiapps@gmail.com";
        /// <summary>
        /// Timespan of cache used by the ewb client.
        /// </summary>
        public static int RazorCacheTimespanInMinutes = 5;
        #endregion

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
        /// <summary>
        /// Maximum depth to which the references will be displayed.
        /// </summary>
        public static int MaxDepthOfDisplayedReferences = 3;
        #endregion

        #region Constants
        /// <summary>
        /// Prefix for storing descriptors in the cache.
        /// </summary>
        public static string CacheDescriptorPrefix => "Descriptor_";
        /// <summary>
        /// Prefix for storing rights in the cache.
        /// </summary>
        public static string CacheRightsPrefix => "Rights_";
        /// <summary>
        /// Name of the JWT claim containing application id.
        /// </summary>
        public static string JWTClaimApplicationId => "ApplicationId";
        /// <summary>
        /// Name of the JWT claim containing user id.
        /// </summary>
        public static string JWTClaimUserId => "UserId";
        /// <summary>
        /// Id under which is the JWT token stored in a session.
        /// </summary>
        public static string SessionJWTKey => "sessionJWT";
        #endregion
    }
}