using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Models;

/// <summary></summary>
public class TKPermutatedContentItemViewModel
{
    /// <summary></summary>
    public string Title { get; set; }

    /// <summary></summary>
    public string MainUrl { get; set; }

    /// <summary></summary>
    public string Description { get; set; }

    /// <summary>
    /// Optional image url.
    /// </summary>
    public string ImageUrl { get; set; }

    /// <summary></summary>
    public List<TKPermutatedContentLinkViewModel> AdditionalUrls { get; set; }

    /// <summary></summary>
    public TKPermutatedContentItemViewModel(string title, string mainUrl)
    {
        Title = title;
        MainUrl = mainUrl;
    }
}
