#if NETFULL
using HealthCheck.Core.Modules.RequestLog.Models;
using HealthCheck.RequestLog.Enums;
using System;
using System.Linq;

namespace HealthCheck.RequestLog.Services
{
    /// <summary>
    /// Options for the <see cref="RequestLogService"/>.
    /// </summary>
    public class RequestLogServiceOptions
    {
        /// <summary>
        /// Max number of successfull calls to store.
        /// </summary>
        public int MaxCallCount { get; set; } = 1;

        /// <summary>
        /// Max number of failing calls to store.
        /// </summary>
        public int MaxErrorCount { get; set; } = 3;

        /// <summary>
        /// Policy for successfull call storage.
        /// </summary>
        public RequestLogCallStoragePolicy CallStoragePolicy { get; set; }

        /// <summary>
        /// Policy for failed call storage.
        /// </summary>
        public RequestLogCallStoragePolicy ErrorStoragePolicy { get; set; }

        /// <summary>
        /// Version of the application. Is stored along with the any calls/errors.
        /// </summary>
        public string ApplicationVersion { get; set; }

        /// <summary>
        /// Optionally create a group name from the given controller type.
        /// <para>Defaults to last part of namespace if it does not start with 'controller'.</para>
        /// </summary>
        public Func<Type, string> ControllerGroupNameFactory { get; set; } = (ctype) =>
        {
            var ns = ctype?.Namespace;
            var lastNsPart = ns?.Split('.')?.LastOrDefault();
            return (lastNsPart?.ToLower()?.StartsWith("controller") == true)
                ? null
                : lastNsPart;
        };

        /// <summary>
        /// Optionally define custom logic to filter out requests.
        /// <para>Return false to exclude from the log.</para>
        /// </summary>
        public Func<LogFilterEvent, bool> RequestEventFilter { get; set; }
    }
}
#endif
