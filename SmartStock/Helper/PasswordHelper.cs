using System;
using System.Security.Cryptography;
using System.Text;

namespace SmartStock.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashes a plain text password using SHA256.
        /// Simple and dependency-free, appropriate for this project's scope.
        /// </summary>
        public static string HashPassword(string plainTextPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plainTextPassword);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Compares a plain text password against a stored hash.
        /// </summary>
        public static bool VerifyPassword(string plainTextPassword, string storedHash)
        {
            string hashOfInput = HashPassword(plainTextPassword);
            return string.Equals(hashOfInput, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}