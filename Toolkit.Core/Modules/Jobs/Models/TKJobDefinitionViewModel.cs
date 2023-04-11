using QoDL.Toolkit.Core.Util.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Jobs.Models;

/// <summary></summary>
public class TKJobDefinitionViewModel
{
    /// <summary></summary>
    public string Id { get; set; }

    /// <summary></summary>
    public string Name { get; set; }

    /// <summary></summary>
    public string Description { get; set; }

    /// <summary>
    /// Name of the group this job belongs to if any.
    /// </summary>
    public string GroupName { get; set; }

    /// <inheritdoc />
    public virtual List<string> Categories { get; set; } = new();

    /// <summary></summary>
    public bool SupportsStart { get; set; }

    /// <summary></summary>
    public bool SupportsStop { get; set; }

    /// <summary></summary>
    public bool HasCustomParameters { get; set; }

    /// <summary></summary>
    public List<TKBackendInputConfig> CustomParameters { get; set; }
}
