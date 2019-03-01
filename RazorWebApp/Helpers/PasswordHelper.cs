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
        static HashAlgorithm hashAlgorithm = new SHA384Managed();
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
 
        public static bool CheckHash(string password, string hash)
        {
            // Compute hash for password
            string expectedHash = ComputeHash(password);
            // Check if computed hash == hash from parameter
            return (hash == expectedHash);
        }
        public static string GenerateRandomPassword(int length)
        {
            //TODO
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