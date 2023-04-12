using QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models;

/// <summary>
/// Any additional details about an item.
/// </summary>
public class TKDataRepeaterStreamItemDetails
{
    /// <summary>
    /// Any relevant description. Supports HTML.
    /// </summary>
    public string DescriptionHtml { get; set; }

    /// <summary>
    /// Any relevant links.
    /// </summary>
    public List<TKDataRepeaterStreamItemHyperLink> Links { get; set; }
}
