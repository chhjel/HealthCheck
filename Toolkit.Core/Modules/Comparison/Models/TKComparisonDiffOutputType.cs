namespace QoDL.Toolkit.Core.Modules.Comparison.Models
{
    /// <summary></summary>
    public enum TKComparisonDiffOutputType
    {
        /// <summary>
        /// Monaco-diff.
        /// </summary>
        Diff = 0,

        /// A note.
        Note,

        /// <summary>
        /// A note for each side.
        /// </summary>
        SideNotes,

        /// Custom html.
        Html,

        /// <summary>
        /// Custom html for each side.
        /// </summary>
        SideHtml
    }
}
