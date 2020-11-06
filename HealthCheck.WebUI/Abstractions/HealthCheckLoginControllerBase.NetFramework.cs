#if NETFULL
using HealthCheck.Core.Attributes;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Util;
using System.Web;
using System.Web.Mvc;

namespace HealthCheck.WebUI.Abstractions
{
    /// <summary>
    /// Base controller for integrated healthcheck login.
    /// </summary>
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
        public virtual ActionResult Login(HCIntegratedLoginRequest model)
        {
            if (!Enabled) return HttpNotFound();
            
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
