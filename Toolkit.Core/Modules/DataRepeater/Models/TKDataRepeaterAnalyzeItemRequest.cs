using System;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary></summary>
public class TKDataRepeaterAnalyzeItemRequest
{
    /// <summary>
    /// Type of the stream.
    /// </summary>
    public string StreamId { get; set; }

    /// <summary></summary>
    public Guid ItemId { get; set; }
}
