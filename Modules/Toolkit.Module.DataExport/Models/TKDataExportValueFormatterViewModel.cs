using QoDL.Toolkit.Core.Util.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport.Models;

/// <summary></summary>
public class TKDataExportValueFormatterViewModel
{
    /// <summary></summary>
    public string Id { get; set; }

    /// <summary></summary>
    public string Name { get; set; }

    /// <summary></summary>
    public string Description { get; set; }

    /// <summary></summary>
    public List<string> SupportedTypes { get; set; }

    /// <summary></summary>
    public List<TKBackendInputConfig> CustomParameterDefinitions { get; set; }
}
