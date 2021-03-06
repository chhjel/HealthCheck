#if NETCORE
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.TFA.Util;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheck.WebUI.TFA
{
    /// <summary>
    /// Base controller for integrated healthcheck login.
    /// </summary>
    [Route("[controller]")]
    public abstract class HealthCheckLogin2FAControllerBase : HealthCheckLoginControllerBase
    {
        #region Helpers
        /// <summary>
        /// Validate 2FA user input.
        /// </summary>
        /// <param name="userSecret">The 2FA secret for the user attempting to login.</param>
        /// <param name="code">Code user entered in the login dialog.</param>
        protected bool Validate2FACode(string userSecret, string code) => HealthCheck2FAUtil.Validate2FACode(userSecret, code);
#endregion
    }
}
#endif
