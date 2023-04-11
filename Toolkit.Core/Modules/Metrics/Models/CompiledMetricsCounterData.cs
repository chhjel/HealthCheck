using System;

namespace QoDL.Toolkit.Core.Modules.Metrics.Models;

/// <summary>
/// Metrics data prepared for frontend consumption.
/// </summary>
public class CompiledMetricsCounterData
{
    /// <summary>
    /// Id of this value type.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Total value.
    /// </summary>
    public long Value { get; set; }

    /// <summary>
    /// When the latest change was reported.
    /// </summary>
    public DateTimeOffset LastChanged { get; set; }

    /// <summary>
    /// When the first data with this id was stored.
    /// </summary>
    public DateTimeOffset FirstStored { get; set; }

    /// <summary></summary>
    public override string ToString() => $"{Id}: {Value}";
}
