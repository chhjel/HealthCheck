using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.RequestLog.Models;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Settings
{
    /// <summary>
    /// Module for executing tests at runtime.
    /// </summary>
    public class HCRequestLogModule : HealthCheckModuleBase<HCRequestLogModule.AccessOption>
    {
        private HCRequestLogModuleOptions Options { get; }

        /// <summary>
        /// Module for executing tests at runtime.
        /// </summary>
        public HCRequestLogModule(HCRequestLogModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCRequestLogModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            Nothing = 0,

            /// <summary>Allowed to clear requestlog.</summary>
            ClearLog = 1
        }

        #region Invokable methods
        /// <summary>
        /// Get all request log actions.
        /// </summary>
        [HealthCheckModuleMethod]
        public List<LoggedEndpointDefinition> GetRequestLog()
        {
            return Options.RequestLogService?.GetRequests() ?? new List<LoggedEndpointDefinition>();
        }

        /// <summary>
        /// Clears the requestlog.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ClearLog)]
        public string ClearRequestLog(HealthCheckModuleContext context, bool includeDefinitions = false)
        {
            var auditAction = (includeDefinitions) ? "Cleared request log + definitions" : "Cleared request log";
            context.AddAuditEvent(action: auditAction);
            Options.RequestLogService?.ClearRequests(includeDefinitions);

            return "Cleared";
        }
        #endregion
    }
}
