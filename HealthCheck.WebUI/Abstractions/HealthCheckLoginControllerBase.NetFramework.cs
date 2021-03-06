#if NETFULL
using HealthCheck.Core.Attributes;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Util;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HealthCheck.WebUI.Abstractions
{
    /// <summary>
    /// Base controller for integrated healthcheck login.
    /// </summary>
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

        private HealthCheckLoginControllerHelper Helper { get; set; } = new HealthCheckLoginControllerHelper();

        #region Endpoints
        /// <summary>
        /// Attempts to login using custom logic from <see cref="HandleLoginRequest"/>.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        public virtual async Task<ActionResult> Login(HCIntegratedLoginRequest model)
        {
            if (!Enabled) return HttpNotFound();
            
            if (MinAddedDelay != null && MaxAddedDelay != null)
            {
                var random = new Random();
                var msDelay = random.Next((int)MinAddedDelay.Value.TotalMilliseconds, (int)MaxAddedDelay.Value.TotalMilliseconds);
                await Task.Delay(msDelay);
            }

            var result = HandleLoginRequest(model);
            if (result == null) return HttpNotFound();

            return CreateJsonResult(result);
        }
        #endregion

        #region Overridables
        /// <summary>
        /// Handle login request here.
        /// </summary>
        protected abstract HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request);
        #endregion

        #region Helpers
        /// <summary>
        /// Serializes the given object into a json result.
        /// </summary>
        protected ActionResult CreateJsonResult(object obj)
            => Content(Helper.SerializeJson(obj), "application/json");
        #endregion
    }
}
#endif
