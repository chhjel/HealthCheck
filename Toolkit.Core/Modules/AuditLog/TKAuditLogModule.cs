using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.AuditLog.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static QoDL.Toolkit.Core.Modules.AuditLog.TKAuditLogModuleOptions;

namespace QoDL.Toolkit.Core.Modules.AuditLog;

/// <summary>
/// Module for viewing audit logs.
/// </summary>
public class TKAuditLogModule : ToolkitModuleBase<TKAuditLogModule.AccessOption>
{
    /// <summary>
    /// Retrieve the service from the Options object.
    /// </summary>
    public IAuditEventStorage AuditEventService => Options.AuditEventService;

    /// <summary>
    /// Optional logic for stripping any sensitive data.
    /// <para>Various usefull util methods can be found in <see cref="TKSensitiveDataUtils"/>.</para>
    /// <para>Retrieved from the Options object.</para>
    /// </summary>
    public StripSensitiveDataDelegate SensitiveDataStripper => Options.SensitiveDataStripper;

    private TKAuditLogModuleOptions Options { get; }
    internal bool IncludeClientConnectionDetailsInAllEvents => Options?.IncludeClientConnectionDetailsInAllEvents == true;

    /// <summary>
    /// Module for viewing audit logs.
    /// </summary>
    public TKAuditLogModule(TKAuditLogModuleOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// Check options object for issues.
    /// </summary>
    public override IEnumerable<string> Validate()
    {
        var issues = new List<string>();
        if (Options.AuditEventService == null) issues.Add("Options.AuditEventService must be set.");
        return issues;
    }

    /// <summary>
    /// Get frontend options for this module.
    /// </summary>
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKAuditLogModuleConfig();

    /// <summary>
    /// Different access options for this module.
    /// </summary>
    [Flags]
    public enum AccessOption
    {
        /// <summary>Does nothing.</summary>
        None = 0,

        /// <summary>Allow access to view blobs stored along audit events.</summary>
        ViewBlobs = 1
    }

    #region Invokable methods
    /// <summary>
    /// Get filtered audit events to show in the UI.
    /// </summary>
    [ToolkitModuleMethod]
    public async Task<IEnumerable<AuditEventViewModel>> GetFilteredAudits(AuditEventFilterInputData filter = null)
    {
        var from = filter?.FromFilter ?? DateTimeOffset.MinValue;
        var to = filter?.ToFilter ?? DateTimeOffset.MaxValue;
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
                Blobs = getEventBlobs(x),
                UserId = x.UserId,
                UserName = x.UserName,
                UserAccessRoles = x.UserAccessRoles
            });

        List<KeyValuePair<string, Guid>> getEventBlobs(AuditEvent x)
        {
            if (Options?.AuditEventService?.SupportsBlobs() != true)
            {
                return new List<KeyValuePair<string, Guid>>();
            }
            return x.BlobIds ?? new List<KeyValuePair<string, Guid>>();
        }
    }

    /// <summary>
    /// Get filtered audit events to show in the UI.
    /// </summary>
    [ToolkitModuleMethod(requiresAccessTo: AccessOption.ViewBlobs)]
    public async Task<string> GetAuditBlobContents(Guid id)
        => await Options.AuditEventService.GetBlob(id);
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
