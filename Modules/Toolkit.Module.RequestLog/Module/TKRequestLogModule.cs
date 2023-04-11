using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.RequestLog.Models;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Settings
{
    /// <summary>
    /// Module for executing tests at runtime.
    /// </summary>
    public class TKRequestLogModule : ToolkitModuleBase<TKRequestLogModule.AccessOption>
    {
        private TKRequestLogModuleOptions Options { get; }

        /// <summary>
        /// Module for executing tests at runtime.
        /// </summary>
        public TKRequestLogModule(TKRequestLogModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            if (Options.RequestLogService == null) issues.Add("Options.RequestLogService must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKRequestLogModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,

            /// <summary>Allowed to clear requestlog.</summary>
            ClearLog = 1
        }

        #region Invokable methods
        /// <summary>
        /// Get all request log actions.
        /// </summary>
        [ToolkitModuleMethod]
        public List<LoggedEndpointDefinition> GetRequestLog()
        {
            return Options.RequestLogService?.GetRequests() ?? new List<LoggedEndpointDefinition>();
        }

        /// <summary>
        /// Clears the requestlog.
        /// </summary>
        [ToolkitModuleMethod(AccessOption.ClearLog)]
        public string ClearRequestLog(ToolkitModuleContext context, bool includeDefinitions = false)
        {
            var auditAction = (includeDefinitions) ? "Cleared request log + definitions" : "Cleared request log";
            context.AddAuditEvent(action: auditAction);
            Options.RequestLogService?.ClearRequests(includeDefinitions);

            return "Cleared";
        }
        #endregion
    }
}
