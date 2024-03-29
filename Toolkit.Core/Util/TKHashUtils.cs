using System;
using System.Security.Cryptography;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Utils related to hashes.
/// </summary>
public static class TKHashUtils
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int IterationCount = 10000;

    /// <summary>
    /// Generate b64 of Rfc2898 hash and salt for the given input.
    /// </summary>
    public static string GenerateHash(string input, out string salt)
    {
        using Rfc2898DeriveBytes rfc2898DeriveBytes = new(input, SaltSize)
        {
            IterationCount = IterationCount
        };

        var hashData = rfc2898DeriveBytes.GetBytes(HashSize);
        var saltData = rfc2898DeriveBytes.Salt;
        salt = Convert.ToBase64String(saltData);
        return Convert.ToBase64String(hashData);
    }

    /// <summary>
    /// Validate hash and salt generated by <see cref="GenerateHash"/>,
    /// </summary>
    public static bool ValidateHash(string input, string inputHash, string salt)
    {
        using Rfc2898DeriveBytes rfc2898DeriveBytes = new(input, SaltSize)
        {
            IterationCount = IterationCount,
            Salt = Convert.FromBase64String(salt)
        };

        var hashData = rfc2898DeriveBytes.GetBytes(HashSize);
        return Convert.ToBase64String(hashData) == inputHash;
    }
}
