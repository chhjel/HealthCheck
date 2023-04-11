using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport.Models;

/// <summary>
/// Definition of an item model.
/// </summary>
public class TKDataExportStreamItemDefinition
{
    /// <summary>
    /// Id of the related stream.
    /// </summary>
    public string StreamId { get; set; }

    /// <summary>
    /// Name of the stream item.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// All members of the item.
    /// </summary>
    public List<TKDataExportStreamItemDefinitionMember> Members { get; set; } = new();
}
