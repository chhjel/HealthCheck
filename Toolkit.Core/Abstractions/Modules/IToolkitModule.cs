using QoDL.Toolkit.Core.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Abstractions.Modules;

/// <summary>
/// Inherit from <see cref="ToolkitModuleBase{TModuleAccessOptionsEnum}"/> instead of this one.
/// </summary>
public interface IToolkitModule
{
    /// <summary>
    /// All categories defined for the module if any.
    /// </summary>
    List<string> AllCategories { get; }

    /// <summary>
    /// All ids of things that can be given access to individually.
    /// </summary>
    List<TKModuleIdData> AllIds { get; }
}
