using QoDL.Toolkit.Core.Modules.GoTo.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.GoTo.Abstractions;

/// <summary>
/// Handles attempting resolving a thing to an instance of something.
/// </summary>
public interface ITKGoToResolver
{
    /// <summary>
    /// Name of the resolver/type.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Try to locate a thing with the given value.
    /// </summary>
    Task<TKGoToResolvedData> TryResolveAsync(string input);
}
