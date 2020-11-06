#if NETCORE
using HealthCheck.Core.Attributes;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Util;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheck.WebUI.Abstractions
{
    /// <summary>
    /// Base controller for integrated healthcheck login.
    /// </summary>
    [Route("[controller]")]
    public abstract class HealthCheckLoginControllerBase: Controller
    {
        /// <summary>
        /// Set to false to return 404 for all actions.
        /// <para>Enabled by default.</para>
        /// </summary>
        protected bool Enabled { get; set; } = true;

        private HealthCheckLoginControllerHelper Helper { get; set; } = new HealthCheckLoginControllerHelper();

        #region Endpoints
        /// <summary>
        /// Attempts to login using custom logic from <see cref="HandleLoginRequest"/>.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("Login")]
        public virtual ActionResult Login(HCIntegratedLoginRequest model)
        {
            if (!Enabled) return NotFound();
            
            var result = HandleLoginRequest(model);
            if (result == null) return NotFound();

            return CreateJsonResult(result);
        }
        #endregion

        #region Overridables
        /// <summary>
        /// Handle login request here.
        /// </summary>
        protected virtual HCIntegratedLoginResult HandleLoginRequest(HCIntegratedLoginRequest request) => null;
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
