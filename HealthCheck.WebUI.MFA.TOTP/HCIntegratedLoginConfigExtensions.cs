using HealthCheck.WebUI.Models;

namespace HealthCheck.WebUI.MFA.TOTP
{
    /// <summary>
    /// TOTP extensions for <see cref="HCIntegratedLoginConfig"/>.
    /// </summary>
    public static class HCIntegratedLoginConfigExtensions
    {
        /// <summary>
        /// Enable time based one time password with an extra input field.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="required">If false, the input won't be required in the frontend.</param>
        public static HCIntegratedLoginConfig EnableTOTP(this HCIntegratedLoginConfig config, bool required = true)
        {
            config.EnableTimeBasedOneTimePassword(
                current2FACodeExpirationTime: HCMfaTotpUtil.GetCurrentTotpCodeExpirationTime(),
                required: required
            );
            return config;
        }
    }
}
