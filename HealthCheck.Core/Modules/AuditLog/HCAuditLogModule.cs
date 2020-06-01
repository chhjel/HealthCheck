using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
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
        /// <summary>
        /// Retrieve the service from the Options object.
        /// </summary>
        public IAuditEventStorage AuditEventService => Options.AuditEventService;

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
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCAuditLogModuleConfig();
        
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
            else if (filter.FromFilter != null && e.Timestamp.ToUniversalTime() < filter.FromFilter?.ToUniversalTime()) return false;
            else if (filter.ToFilter != null && e.Timestamp.ToUniversalTime() > filter.ToFilter?.ToUniversalTime()) return false;
            else if (!string.IsNullOrWhiteSpace(filter.SubjectFilter) && e.Subject?.ToLower()?.Contains(filter.SubjectFilter?.ToLower()) != true) return false;
            else if (!string.IsNullOrWhiteSpace(filter.ActionFilter) && e.Action?.ToLower()?.Contains(filter.ActionFilter?.ToLower()) != true) return false;
            else if (!string.IsNullOrWhiteSpace(filter.UserIdFilter) && e.UserId?.ToLower()?.Contains(filter.UserIdFilter?.ToLower()) != true) return false;
            else if (!string.IsNullOrWhiteSpace(filter.UserNameFilter) && e.UserName?.ToLower()?.Contains(filter.UserNameFilter?.ToLower()) != true) return false;
            else if (!string.IsNullOrWhiteSpace(filter.AreaFilter) && e.Area != filter.AreaFilter) return false;
            else return true;
        }
        #endregion
    }
}
