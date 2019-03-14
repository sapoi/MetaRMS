using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
 
namespace SharedLibrary.Helpers
{
    // backend
    public class PasswordHelper
    {
        /// <summary>
        /// Algorithm used for password hash.
        /// </summary>
        static HashAlgorithm hashAlgorithm = new SHA384Managed();
        /// <summary>
        /// This method computes hash of string from parameter.
        /// </summary>
        /// <param name="password">Password to be hashed</param>
        /// <returns>Hashed password.</returns>
        public static string ComputeHash(string password)
        {
            // Convert password into a byte array
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            // Get hash algorithm and compute 
            byte[] hashBytes = hashAlgorithm.ComputeHash(passwordBytes);
            // Convert result into a base64-encoded string
            string hashedPassword = Convert.ToBase64String(hashBytes);
            // Return hashed password with salt
            return hashedPassword;
        }
        /// <summary>
        /// This method checks if hashed password from parametres is eaqual to the hash from the parametres.
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <param name="hash">Hash to validate password against</param>
        /// <returns>True if ashed password and hash are eaqual, false otherwise.</returns>
        public static bool CheckHash(string password, string hash)
        {
            // Compute hash for password
            string expectedHash = ComputeHash(password);
            // Check if computed hash == hash from parameter
            return (hash == expectedHash);
        }
        /// <summary>
        /// This method returns a new password of length as the length parameter.
        /// </summary>
        /// <param name="length">Length of the password</param>
        /// <returns>New random password.</returns>
        public static string GenerateRandomPassword(int length)
        {
            // Valid characters to genetare the password from
            const string valid = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
 
    }
}