#if NETCORE
using HealthCheck.Core.Attributes;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Util;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.Abstractions
{
    /// <summary>
    /// Base controller for integrated healthcheck login.
    /// </summary>
    [Route("[controller]")]
    public abstract class HealthCheckLoginControllerBase : Controller
    {
        /// <summary>
        /// Set to false to return 404 for all actions.
        /// <para>Enabled by default.</para>
        /// </summary>
        protected bool Enabled { get; set; } = true;

        /// <summary>
        /// Minimum added delay on login to limit brute force.
        /// </summary>
        protected TimeSpan? MinAddedDelay { get; set; } = TimeSpan.FromSeconds(1.5);

        /// <summary>
        /// Maximum added delay on login to limit brute force.
        /// </summary>
        protected TimeSpan? MaxAddedDelay { get; set; } = TimeSpan.FromSeconds(3);

        private readonly HealthCheckLoginControllerHelper _helper = new HealthCheckLoginControllerHelper();

        #region Endpoints
        /// <summary>
        /// Attempts to login using custom logic from <see cref="HandleLoginRequest"/>.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("Login")]
        public virtual async Task<ActionResult> Login(HCIntegratedLoginRequest model)
        {
            if (!Enabled) return NotFound();
            await Delay();
            
            var result = HandleLoginRequest(model);
            if (result == null) return NotFound();

            return CreateJsonResult(result);
        }

        /// <summary>
        /// Invoked when requesting a 2FA code.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        public virtual async Task<ActionResult> Request2FACode(HCIntegratedLoginRequest2FACodeRequest model)
        {
            if (!Enabled) return NotFound();
            await Delay();

            var result = Handle2FACodeRequest(model);
            if (result == null) return NotFound();

            return CreateJsonResult(result);
        }
        #endregion

        #region Overridables
        /// <summary>
        /// Handle login request here.
        /// </summary>
        protected virtual HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request) => null;

        /// <summary>
        /// Optionally handle 2FA code request here.
        /// </summary>
        protected virtual HCIntegratedLogin2FACodeRequestResult Handle2FACodeRequest(HCIntegratedLoginRequest2FACodeRequest request) => null;
        #endregion

        #region Helpers
        /// <summary>
        /// Serializes the given object into a json result.
        /// </summary>
        protected ActionResult CreateJsonResult(object obj)
            => Content(_helper.SerializeJson(obj), "application/json");

        /// <summary>
        /// Delay by the configured amount.
        /// </summary>
        protected async Task Delay()
        {
            if (MinAddedDelay != null && MaxAddedDelay != null)
            {
                var random = new Random();
                var msDelay = random.Next((int)MinAddedDelay.Value.TotalMilliseconds, (int)MaxAddedDelay.Value.TotalMilliseconds);
                await Task.Delay(msDelay);
            }
        }
        #endregion
    }
}
#endif
