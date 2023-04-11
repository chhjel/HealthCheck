namespace QoDL.Toolkit.Core.Modules.Dataflow.Models;

/// <summary>
/// Model sent to ToolkitControllerBase.GetDataflowStreamEntries
/// </summary>
public class GetDataflowStreamEntriesFilter
{
    /// <summary>
    /// Id of the stream to get data from.
    /// </summary>
    public string StreamId { get; set; }

    /// <summary>
    /// Filter passed to the stream.
    /// </summary>
    public DataflowStreamFilter StreamFilter { get; set; }
}
