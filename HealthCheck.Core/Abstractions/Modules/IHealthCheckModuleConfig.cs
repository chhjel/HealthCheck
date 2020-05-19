using System.Collections.Generic;

namespace HealthCheck.Core.Abstractions.Modules
{
    /// <summary>
    /// Page-options for the module.
    /// </summary>
    public interface IHealthCheckModuleConfig
    {
        /// <summary>
        /// Name of the module.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Vue component name for the module page in frontend.
        /// </summary>
        string ComponentName { get; }

        /// <summary>
        /// Any custom link-tags to include.
        /// </summary>
        List<HealthCheckLinkTagModel> LinkTags { get; }

        /// <summary>
        /// Any custom script-tags to include.
        /// </summary>
        List<HealthCheckScriptTagModel> ScriptTags { get; }
    }
}