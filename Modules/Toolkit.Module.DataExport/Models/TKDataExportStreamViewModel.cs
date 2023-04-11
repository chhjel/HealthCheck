using QoDL.Toolkit.Core.Util.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport.Models;

/// <summary></summary>
public class TKDataExportStreamViewModel
{
    /// <summary></summary>
    public string Id { get; set; }

    /// <summary></summary>
    public string Name { get; set; }

    /// <summary></summary>
    public string Description { get; set; }

    /// <summary></summary>
    public string GroupName { get; set; }

    /// <summary></summary>
    public bool ShowQueryInput { get; set; }

    /// <summary></summary>
    public bool AllowAnyPropertyName { get; set; }

    /// <summary></summary>
    public TKDataExportStreamItemDefinitionViewModel ItemDefinition { get; set; }

    /// <summary></summary>
    public List<TKBackendInputConfig> CustomParameterDefinitions { get; set; }

    /// <summary></summary>
    public List<TKDataExportValueFormatterViewModel> ValueFormatters { get; set; }
}
