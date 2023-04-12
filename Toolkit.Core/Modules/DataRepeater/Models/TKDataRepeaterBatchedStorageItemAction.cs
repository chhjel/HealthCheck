using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary></summary>
public class TKDataRepeaterBatchedStorageItemAction
{
    /// <summary></summary>
    public ITKDataRepeaterStreamItem Item { get; set; }

    /// <summary></summary>
    public object Hint { get; set; }

    /// <summary></summary>
    public TKDataRepeaterBatchedStorageItemAction(ITKDataRepeaterStreamItem item, object hint)
    {
        Item = item;
        Hint = hint;
    }
}
