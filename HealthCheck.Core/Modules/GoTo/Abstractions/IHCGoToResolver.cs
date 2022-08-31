using HealthCheck.Core.Modules.GoTo.Models;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.GoTo.Abstractions
{
    /// <summary>
    /// Handles attempting resolving a thing to an instance of something.
    /// </summary>
    public interface IHCGoToResolver
    {
        /// <summary>
        /// Name of the resolver/type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Try to locate a thing with the given value.
        /// </summary>
        Task<HCGoToResolvedData> TryResolveAsync(string input);
    }
}
