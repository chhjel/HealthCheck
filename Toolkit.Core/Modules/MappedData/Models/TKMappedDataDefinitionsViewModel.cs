using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.MappedData.Models;

/// <summary></summary>
public class TKMappedDataDefinitionsViewModel
{
    /// <summary></summary>
    public List<TKMappedClassDefinitionViewModel> ClassDefinitions { get; set; } = new();

    /// <summary></summary>
    public List<TKMappedReferencedTypeDefinitionViewModel> ReferencedDefinitions { get; set; } = new();
}
