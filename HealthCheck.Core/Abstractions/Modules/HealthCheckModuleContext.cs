using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.AuditLog.Models;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        /// Details about the current request.
        /// </summary>
        public HealthCheckModuleRequestData Request { get; set; }

        /// <summary>
        /// List of modules the request has access to.
        /// </summary>
        public List<ModuleAccess> CurrentRequestModulesAccess { get; set; }

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
        /// Default value if pageAccess is null, false if no roles were given.
        /// </summary>
        public bool HasRoleAccess<TAccessRole>(Maybe<TAccessRole> roles, bool defaultValue = true)
        {
            // No access defined => default
            if (roles == null || !roles.HasValue)
            {
                return defaultValue;
            }
            // Access is defined but no user roles => denied
            else if ((CurrentRequestRoles == null || (int)CurrentRequestRoles == 0) && roles.HasValue())
            {
                return false;
            }

            return EnumUtils.EnumFlagHasAnyFlagsSet(CurrentRequestRoles, roles.Value);
        }

        internal bool HasAnyOfRoles(object roles)
        {
            if (roles == null || ((int)roles) == 0)
            {
                return false;
            }

            return EnumUtils.EnumFlagHasAnyFlagsSet(CurrentRequestRoles, roles);
        }

        /// <summary>
        /// All registered audit events.
        /// </summary>
        public List<AuditEvent> AuditEvents { get; } = new List<AuditEvent>();

        /// <summary>
        /// All currently loaded modules.
        /// </summary>
        public ReadOnlyCollection<HealthCheckLoadedModule> LoadedModules { get; set; }

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
                Timestamp = DateTimeOffset.Now,
                UserId = UserId,
                UserName = UserName,
                UserAccessRoles = EnumUtils.TryGetEnumFlaggedValueNames(CurrentRequestRoles)
            });
        }

        /// <summary>
        /// Access level to a module.
        /// </summary>
        public class ModuleAccess
        {
            /// <summary>
            /// Id of module that this is access for.
            /// </summary>
            public string ModuleId { get; set; }

            /// <summary>
            /// Stringified enum values that the request has access to.
            /// </summary>
            public List<string> AccessOptions { get; set; }
        }
    }
}
