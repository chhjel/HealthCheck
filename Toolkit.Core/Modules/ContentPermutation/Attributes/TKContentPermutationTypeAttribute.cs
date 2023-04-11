using QoDL.Toolkit.Core.Modules.ContentPermutation.Models;
using System;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Attributes;

/// <summary>
/// Place on your class containing properties to permutate.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class TKContentPermutationTypeAttribute : Attribute
{
    /// <summary>
    /// Optionally override the display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Optional description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Requested <see cref="TKGetContentPermutationContentOptions.MaxCount"/> will be limited by this number. Defaults to 8.
    /// </summary>
    public int MaxAllowedContentCount { get; set; } = 8;

    /// <summary>
    /// Default value for <see cref="TKGetContentPermutationContentOptions.MaxCount"/> will be limited by this number. Defaults to 8.
    /// </summary>
    public int DefaultContentCount { get; set; } = 8;
}
