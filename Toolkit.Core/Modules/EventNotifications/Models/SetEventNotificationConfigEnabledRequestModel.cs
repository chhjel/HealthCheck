using System;

namespace QoDL.Toolkit.Core.Modules.EventNotifications.Models;

/// <summary>
/// Model sent to module method.
/// </summary>
public class SetEventNotificationConfigEnabledRequestModel
{
    /// <summary>
    /// Id of the config.
    /// </summary>
    public Guid ConfigId { get; set; }

    /// <summary>
    /// True to enable, false to disable.
    /// </summary>
    public bool Enabled { get; set; }
}
