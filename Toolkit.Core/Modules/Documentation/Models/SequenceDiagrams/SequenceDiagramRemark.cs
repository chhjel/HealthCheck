namespace QoDL.Toolkit.Core.Modules.Documentation.Models.SequenceDiagrams;

/// <summary>
/// A remark for a diagram step.
/// </summary>
public class SequenceDiagramRemark
{
    /// <summary>
    /// Number of the remark. Starts from 1 per diagram.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Remark text.
    /// </summary>
    public string Text { get; set; }
}
