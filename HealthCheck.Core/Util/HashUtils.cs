using System;
using System.Security.Cryptography;

namespace HealthCheck.Core.Util
{
    internal static class HashUtils
    {
        private const int SaltSize = 32;
        private const int HashSize = 32;
        private const int IterationCount = 10000;

        internal static string GenerateHash(string input, out string salt)
        {
            using Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(input, SaltSize)
            {
                IterationCount = IterationCount
            };

            var hashData = rfc2898DeriveBytes.GetBytes(HashSize);
            var saltData = rfc2898DeriveBytes.Salt;
            salt = Convert.ToBase64String(saltData);
            return Convert.ToBase64String(hashData);
        }

        internal static bool ValidateHash(string input, string inputHash, string salt)
        {
            using Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(input, SaltSize)
            {
                IterationCount = IterationCount,
                Salt = Convert.FromBase64String(salt)
            };

            var hashData = rfc2898DeriveBytes.GetBytes(HashSize);
            return Convert.ToBase64String(hashData) == inputHash;
        }
    }
}
