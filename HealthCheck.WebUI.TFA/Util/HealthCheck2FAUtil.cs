using OtpNet;
using System;

namespace HealthCheck.WebUI.TFA.Util
{
    /// <summary>
    /// Utils related to two-factor authentication.
    /// </summary>
    public static class HealthCheck2FAUtil
    {
        /// <summary>
        /// Get the date when the current code expires.
        /// </summary>
        public static DateTimeOffset GetCurrentCodeExpirationTime()
        {
            var remainingSeconds = new Totp(Base32Encoding.ToBytes("aa")).RemainingSeconds();
            return DateTimeOffset.Now.AddSeconds(remainingSeconds);
        }

        /// <summary>
        /// Checks if the given two factor code input is valid.
        /// </summary>
        public static bool Validate2FACode(string userSecret, string code)
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
        public static string GenerateCode(string userSecret)
        {
            var base32Bytes = Base32Encoding.ToBytes(userSecret);
            var totp = new Totp(base32Bytes);
            return totp.ComputeTotp();
        }

        /// <summary>
        /// Generate a new secret that can be bound to a user.
        /// </summary>
        public static string GenerateOTPSecret()
        {
            var secretKey = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(secretKey);
        }
    }
}
