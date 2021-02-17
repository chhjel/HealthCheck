using System;

namespace HealthCheck.Core.Enums
{
    /// <summary>Type of healthcheck module</summary>
    [Flags]
    public enum HCModuleType
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        Tests = 1 << 0,
        /// <summary></summary>
        AuditLog = 1 << 1,
        /// <summary></summary>
        SecureFileDownload = 1 << 2,
        /// <summary></summary>
        AccessTokens = 1 << 3,
        /// <summary></summary>
        Dataflow = 1 << 4,
        /// <summary></summary>
        Documentation = 1 << 5,
        /// <summary></summary>
        EventNotifications = 1 << 6,
        /// <summary></summary>
        LogViewer = 1 << 7,
        /// <summary></summary>
        Messages = 1 << 8,
        /// <summary></summary>
        Settings = 1 << 9,
        /// <summary></summary>
        SiteEvents = 1 << 10,

        /// <summary></summary>
        EndpointControl = 1 << 11,
        /// <summary></summary>
        Code = 1 << 12,
        /// <summary></summary>
        RequestLog = 1 << 13
    }
}
