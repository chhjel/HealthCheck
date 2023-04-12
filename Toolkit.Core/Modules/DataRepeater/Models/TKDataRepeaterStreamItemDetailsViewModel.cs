using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary>
/// Details about an item.
/// </summary>
public class TKDataRepeaterStreamItemDetailsViewModel
{
    /// <summary>
    /// Any relevant description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Any relevant links.
    /// </summary>
    public List<TKDataRepeaterStreamItemHyperLink> Links { get; set; }

    /// <summary>
    /// The item itself.
    /// </summary>
    public TKDataRepeaterStreamItemViewModel Item { get; set; }
}
