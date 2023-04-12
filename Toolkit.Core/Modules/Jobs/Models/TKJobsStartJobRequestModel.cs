using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Jobs.Models;

/// <summary></summary>
public class TKJobsStartJobRequestModel
{
    /// <summary></summary>
    public string SourceId { get; set; }

    /// <summary></summary>
    public string JobId { get; set; }

    /// <summary></summary>
    public Dictionary<string, string> Parameters { get; set; }
}
