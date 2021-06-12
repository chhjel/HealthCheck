using OtpNet;
using System;

namespace HealthCheck.WebUI.MFA.TOTP
{
    /// <summary>
    /// Utils related to TOTP Multi-Factor Authentication.
    /// </summary>
    public static class HCMfaTotpUtil
    {
        /// <summary>
        /// Get the date when the current TOTP code expires.
        /// </summary>
        public static DateTimeOffset GetCurrentTotpCodeExpirationTime()
        {
            var remainingSeconds = new Totp(Base32Encoding.ToBytes("aa")).RemainingSeconds();
            return DateTimeOffset.Now.AddSeconds(remainingSeconds);
        }

        /// <summary>
        /// Checks if the given TOTP code input is valid.
        /// </summary>
        public static bool ValidateTotpCode(string userSecret, string code)
        {
            if (string.IsNullOrWhiteSpace(userSecret) || string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            var base32Bytes = Base32Encoding.ToBytes(userSecret);
            var totp = new Totp(base32Bytes);
            return totp.VerifyTotp(code, out long _);
        }

        /// <summary>
        /// Generate a new code that can be consumed.
        /// </summary>
        public static string GenerateTotpCode(string userSecret)
        {
            var base32Bytes = Base32Encoding.ToBytes(userSecret);
            var totp = new Totp(base32Bytes);
            return totp.ComputeTotp();
        }

        /// <summary>
        /// Generate a new secret that can be bound to a user.
        /// </summary>
        public static string GenerateTotpSecret()
        {
            var secretKey = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(secretKey);
        }
    }
}
