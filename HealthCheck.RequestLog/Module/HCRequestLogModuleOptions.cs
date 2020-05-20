using HealthCheck.RequestLog.Abstractions;

namespace HealthCheck.Core.Modules.Settings
{
    /// <summary>
    /// Options for <see cref="HCRequestLogModule"/>.
    /// </summary>
    public class HCRequestLogModuleOptions
    {
        /// <summary>
        /// Service used to get and store requests.
        /// </summary>
        public IRequestLogService RequestLogService { get; set; }
    }
}
