using System;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary>
/// Simple log entry.
/// </summary>
public class TKDataRepeaterSimpleLogEntry
{
    /// <summary>
    /// Time of insertion.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Some details.
    /// </summary>
    public string Message { get; set; }
}
