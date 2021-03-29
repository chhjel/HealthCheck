using System;

namespace HealthCheck.Core.Models
{
    /// <summary>
    /// Hint of how to display a input.
    /// </summary>
    [Flags]
    public enum HCUIHint
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Null-values will not be allowed to be entered in the user interface. Does not affect nullable parameters.
        /// </summary>
        NotNull = 1,

        /// <summary>
        /// Only affects strings. Shows a multi-line text area instead of a small input field.
        /// </summary>
        TextArea = 2,

        /// <summary>
        /// Only affects generic lists. Does not allow new entries to be added, or existing entries to be changed.
        /// </summary>
        ReadOnlyList = 4,

        /// <summary>
        /// Make the input field full width in size where possible.
        /// </summary>
        FullWidth = 8
    }
}
