using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Module.DataExport.Models;

/// <summary>
/// Viewmodel for <see cref="TKDataExportQueryResponse"/>
/// </summary>
public class TKDataExportQueryResponseViewModel
{
    /// <summary>
    /// True if nothing failed.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error if any.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Error details if any.
    /// </summary>
    public string ErrorDetails { get; set; }

    /// <summary>
    /// Total count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Items on the current page.
    /// </summary>
    public IEnumerable<object> Items { get; set; } = Enumerable.Empty<object>();

    /// <summary>
    /// Optional note.
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// Optionally force result headers.
    /// </summary>
    public List<TKDataExportStreamItemDefinitionMemberViewModel> AdditionalMembers { get; set; }

    /// <summary>
    /// Creates a new error.
    /// </summary>
    public static TKDataExportQueryResponseViewModel CreateError(string message) => new()
    {
        ErrorMessage = message
    };
}
