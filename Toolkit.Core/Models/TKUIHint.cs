using System;

namespace QoDL.Toolkit.Core.Models;

/// <summary>
/// Hint of how to display a input.
/// </summary>
[Flags]
public enum TKUIHint
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
    FullWidth = 8,

    /// <summary>
    /// Make the input field a full width vscode editor.
    /// </summary>
    CodeArea = 16,

    /// <summary>
    /// If used on DateTime[] or DateTimeOffset[] types, allows for selecting a daterange.
    /// <para>DateTime?[] or DateTimeOffset?[] allows for omitting end.</para>
    /// <para>Index 0 = start selection, index 1 = end selection.</para>
    /// </summary>
    DateRange = 32,

    /// <summary>
    /// Allows frontend to generate a random value. Supported for the following:
    /// <para>Guid inputs - shows a button to generate a new random guid value.</para>
    /// </summary>
    AllowRandom = 64,
}
