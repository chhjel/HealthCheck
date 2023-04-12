using System;

namespace QoDL.Toolkit.Core.Modules.SiteEvents.Models;

/// <summary>
/// Request model sent to <see cref="TKSiteEventsModule.DeleteSiteEvent"/>
/// </summary>
public class DeleteSiteEventRequestModel
{
    /// <summary>
    /// Id of event to delete.
    /// </summary>
    public Guid Id { get; set; }
}
