using HealthCheck.Core.Modules.AuditLog.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Abstractions.Modules
{
    /// <summary>
    /// Context object that can optionally be used as the first parameter in module methods.
    /// </summary>
    public class HealthCheckModuleContext
    {
        /// <summary>
        /// Name of the current method.
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Id of the current user.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Username of the current user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Any access roles of the current request.
        /// </summary>
        public object CurrentRequestRoles { get; set; }

        /// <summary>
        /// Any access options of the current request for the current module.
        /// </summary>
        public object CurrentRequestModuleAccessOptions { get; set; }

        /// <summary>
        /// Get <see cref="CurrentRequestModuleAccessOptions"/> as the given type.
        /// </summary>
        public T GetCurrentRequestModuleAccessOptionsAs<T>() where T : Enum => (T)CurrentRequestModuleAccessOptions;

        /// <summary>
        /// Check if the current request has access to the given module access option.
        /// </summary>
        public bool HasAccess<TAccess>(TAccess access, bool defaultValue = false) where TAccess : Enum
            => EnumUtils.IsFlagSet(CurrentRequestModuleAccessOptions, access, defaultValue);

        /// <summary>
        /// All registered audit events.
        /// </summary>
        public List<AuditEvent> AuditEvents { get; } = new List<AuditEvent>();

        /// <summary>
        /// Register audit events.
        /// </summary>
        public AuditEvent AddAuditEvent(AuditEvent @event) { AuditEvents.Add(@event); return @event; }

        /// <summary>
        /// Register audit events.
        /// </summary>
        public AuditEvent AddAuditEvent(string action, string subject = null)
        {
            return AddAuditEvent(new AuditEvent()
            {
                Area = ModuleName,
                Action = action,
                Subject = subject,
                Timestamp = DateTime.Now,
                UserId = UserId,
                UserName = UserName,
                UserAccessRoles = EnumUtils.TryGetEnumFlaggedValueNames(CurrentRequestRoles)
            });
        }
    }
}
