using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.AuditLog;
using HealthCheck.Core.Modules.AuditLog.Models;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
        /// Id of the current method.
        /// </summary>
        public string ModuleId { get; set; }

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
        /// Access options for the current module.
        /// </summary>
        public ModuleAccess CurrentModuleAccess { get; set; }

        /// <summary>
        /// Access categories for the current module.
        /// </summary>
        public List<string> CurrentModuleCategoryAccess { get; set; }

        /// <summary>
        /// Access ids for the current module.
        /// </summary>
        public List<string> CurrentModuleIdAccess { get; set; }

        /// <summary>
        /// Url to the javascripts for the UI.
        /// </summary>
        public List<string> JavaScriptUrls { get; set; }

        /// <summary>
        /// Url to the assets for the UI.
        /// </summary>
        public List<string> CssUrls { get; set; }

        /// <summary>
        /// Get <see cref="CurrentRequestModuleAccessOptions"/> as the given type.
        /// </summary>
        public T GetCurrentRequestModuleAccessOptionsAs<T>() where T : Enum => (T)CurrentRequestModuleAccessOptions;

        /// <summary>
        /// Logic for stripping any sensitive data if configured.
        /// </summary>
        public HCAuditLogModuleOptions.StripSensitiveDataDelegate SensitiveDataStripper { get; set; }

        /// <summary>
        /// Check if the current request has access to the given module access option.
        /// </summary>
        public bool HasAccess<TAccess>(TAccess access, bool defaultValue = false) where TAccess : Enum
            => EnumUtils.IsFlagSet(CurrentRequestModuleAccessOptions, access, defaultValue);

        /// <summary>
        /// Default value if pageAccess is null, false if no roles were given.
        /// </summary>
        public bool HasRoleAccessObj(object roles, bool defaultValue = true)
        {
            // Not an enum => default
            if (roles is not Enum rolesEnum)
            {
                return defaultValue;
            }
            // Access is defined but no user roles => denied
            else if ((CurrentRequestRoles == null || (int)CurrentRequestRoles == 0))
            {
                return false;
            }

            return EnumUtils.EnumFlagHasAnyFlagsSet(CurrentRequestRoles, rolesEnum);
        }

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
        public AuditEvent AddAuditEvent(AuditEvent @event, bool maskSensitiveData = false)
        {
            if (maskSensitiveData)
            {
                TryStripSensitiveData(@event);
            }
            AuditEvents.Add(@event);
            return @event;
        }

        /// <summary>
        /// Register audit events.
        /// </summary>
        public AuditEvent AddAuditEvent(string action, string subject = null, bool maskSensitiveData = false)
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
            }, maskSensitiveData);
        }

        /// <summary>
        /// Attempt to strip any sensitive data using the configured options on the audit module.
        /// </summary>
        public void TryStripSensitiveData(AuditEvent e)
        {
            if (SensitiveDataStripper != null)
            {
                try
                {
                    e.Subject = SensitiveDataStripper(e.Subject);
                    e.Details = e.Details
                        .Select(x => new KeyValuePair<string, string>(SensitiveDataStripper(x.Key), SensitiveDataStripper(x.Value)))
                        .ToList();
                }
                catch (Exception) { /* Ignored */ }
            }
        }

        /// <summary>
        /// Attempt to strip any sensitive data using the configured options on the audit module.
        /// </summary>
        public string TryStripSensitiveData(string input)
        {
            if (SensitiveDataStripper != null)
            {
                try
                {
                    input = SensitiveDataStripper?.Invoke(input);
                }
                catch (Exception) { /* Ignored */ }
            }
            return input;
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

            /// <summary>
            /// Category values that the request has access to.
            /// <para>Null/empty = all.</para>
            /// </summary>
            public List<string> AccessCategories { get; set; }

            /// <summary>
            /// Ids that the request has access to.
            /// <para>Null/empty = all.</para>
            /// </summary>
            public List<string> AccessIds { get; set; }
        }
    }
}
