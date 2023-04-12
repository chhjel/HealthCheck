using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.MappedData.Models;

/// <summary></summary>
public class TKMappedMemberReferenceDefinitionViewModel
{
    /// <summary></summary>
    public bool Success { get; set; }
    /// <summary></summary>
    public string Error { get; set; }
    /// <summary></summary>
    public string Path { get; set; }
    /// <summary></summary>
    public string RootTypeName { get; set; }
    /// <summary></summary>
    public string RootReferenceId { get; set; }
    /// <summary></summary>
    public List<TKMappedMemberReferencePathItemDefinitionViewModel> Items { get; set; } = new List<TKMappedMemberReferencePathItemDefinitionViewModel>();
}
