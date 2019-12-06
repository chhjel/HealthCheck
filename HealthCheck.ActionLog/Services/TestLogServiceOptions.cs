﻿#if NETFULL
using HealthCheck.ActionLog.Enums;
using System;
using System.Linq;

namespace HealthCheck.ActionLog.Services
{
    /// <summary>
    /// Options for the <see cref="TestLogService"/>.
    /// </summary>
    public class TestLogServiceOptions
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
        public TestLogCallStoragePolicy CallStoragePolicy { get; set; }

        /// <summary>
        /// Policy for failed call storage.
        /// </summary>
        public TestLogCallStoragePolicy ErrorStoragePolicy { get; set; }

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
    }
}
#endif