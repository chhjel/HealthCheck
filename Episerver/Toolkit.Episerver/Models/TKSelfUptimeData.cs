using EPiServer.Data;
using EPiServer.Data.Dynamic;
using System;

namespace QoDL.Toolkit.Episerver.Models;

/// <summary>
/// Model used for storing last checked self uptime data.
/// </summary>
[EPiServerDataStore(AutomaticallyCreateStore = false, AutomaticallyRemapStore = true)]
public class TKSelfUptimeData
{
    /// <summary>
    /// Epi generated id.
    /// </summary>
    public Identity Id { get; set; }

    /// <summary>
    /// When we last performed the check.
    /// </summary>
    public DateTimeOffset LastCheckedAt { get; set; }
}
