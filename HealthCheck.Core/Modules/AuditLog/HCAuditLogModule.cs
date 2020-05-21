using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.AuditLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.AuditLog
{
    /// <summary>
    /// Module for viewing audit logs.
    /// </summary>
    public class HCAuditLogModule : HealthCheckModuleBase<HCAuditLogModule.AccessOption>
    {
        private HCAuditLogModuleOptions Options { get; }

        /// <summary>
        /// Module for viewing audit logs.
        /// </summary>
        public HCAuditLogModule(HCAuditLogModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(AccessOption access) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(AccessOption access) => new HCAuditLogModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            Nothing = 0,
        }

        #region Invokable methods
        /// <summary>
        /// Get filtered audit events to show in the UI.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<IEnumerable<AuditEventViewModel>> GetFilteredAudits(AuditEventFilterInputData filter = null)
        {
            var from = filter?.FromFilter ?? DateTime.MinValue;
            var to = filter?.ToFilter ?? DateTime.MaxValue;
            var events = await Options.AuditEventService.GetEvents(from, to);
            return events
                .Where(x => AuditEventMatchesFilter(x, filter))
                .Select(x => new AuditEventViewModel()
                {
                    Timestamp = x.Timestamp,
                    Area = x.Area,
                    Action = x.Action,
                    Subject = x.Subject,
                    Details = x.Details,
                    UserId = x.UserId,
                    UserName = x.UserName,
                    UserAccessRoles = x.UserAccessRoles,
                });
        }
        #endregion

        #region Private helpers
        private bool AuditEventMatchesFilter(AuditEvent e, AuditEventFilterInputData filter)
        {
            if (filter == null) return true;
            else if (filter.FromFilter != null && e.Timestamp < filter.FromFilter) return false;
            else if (filter.ToFilter != null && e.Timestamp > filter.ToFilter) return false;
            else if (filter.SubjectFilter != null && e.Subject?.ToLower()?.Contains(filter.SubjectFilter?.ToLower()) != true) return false;
            else if (filter.ActionFilter != null && e.Action?.ToLower()?.Contains(filter.ActionFilter?.ToLower()) != true) return false;
            else if (filter.UserIdFilter != null && e.UserId?.ToLower()?.Contains(filter.UserIdFilter?.ToLower()) != true) return false;
            else if (filter.UserNameFilter != null && e.UserName?.ToLower()?.Contains(filter.UserNameFilter?.ToLower()) != true) return false;
            else if (filter.AreaFilter != null && e.Area != filter.AreaFilter) return false;
            else return true;
        }
        #endregion
    }
}
