using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary></summary>
public class TKDataRepeaterStreamViewModel
{
    /// <summary></summary>
    public string Id { get; set; }

    /// <summary></summary>
    public string Name { get; set; }

    /// <summary></summary>
    public string Description { get; set; }

    /// <summary></summary>
    public string StreamItemsName { get; set; }

    /// <summary></summary>
    public string ItemIdName { get; set; }

    /// <summary></summary>
    public string RetryActionName { get; set; }

    /// <summary></summary>
    public string RetryDescription { get; set; }

    /// <inheritdoc />
    public bool ManualAnalyzeEnabled { get; set; }

    /// <inheritdoc />
    public string AnalyzeActionName { get; set; }

    /// <summary></summary>
    public string GroupName { get; set; }

    /// <inheritdoc />
    public virtual List<string> InitiallySelectedTags { get; set; } = new();

    /// <inheritdoc />
    public virtual List<string> FilterableTags { get; set; } = new();

    /// <summary></summary>
    public List<TKDataRepeaterStreamActionViewModel> Actions { get; set; } = new();

    /// <summary></summary>
    public List<TKDataRepeaterStreamBatchActionViewModel> BatchActions { get; set; } = new();
}
