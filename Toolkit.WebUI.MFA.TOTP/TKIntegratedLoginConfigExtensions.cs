using QoDL.Toolkit.WebUI.Models;

namespace QoDL.Toolkit.WebUI.MFA.TOTP
{
    /// <summary>
    /// TOTP extensions for <see cref="TKIntegratedLoginConfig"/>.
    /// </summary>
    public static class TKIntegratedLoginConfigExtensions
    {
        /// <summary>
        /// Enable time based one time password with an extra input field.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="note">Optional note below the input field.</param>
        /// <param name="required">If false, the input won't be required in the frontend.</param>
        public static TKIntegratedLoginConfig EnableTOTP(this TKIntegratedLoginConfig config, string note = null, bool required = true)
        {
            config.EnableTimeBasedOneTimePassword(
                current2FACodeExpirationTime: TKMfaTotpUtil.GetCurrentTotpCodeExpirationTime(),
                required: required,
                note: note
            );
            return config;
        }
    }
}
