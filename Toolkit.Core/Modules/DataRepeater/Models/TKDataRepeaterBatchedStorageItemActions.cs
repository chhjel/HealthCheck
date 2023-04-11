using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary></summary>
public class TKDataRepeaterBatchedStorageItemActions
{
    /// <summary></summary>
    public List<TKDataRepeaterBatchedStorageItemAction> Adds { get; set; } = new();

    /// <summary></summary>
    public List<TKDataRepeaterBatchedStorageItemAction> Updates { get; set; } = new();

    /// <summary></summary>
    public List<TKDataRepeaterBatchedStorageItemAction> Deletes { get; set; } = new();
}
