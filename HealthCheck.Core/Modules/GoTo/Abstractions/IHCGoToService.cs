using HealthCheck.Core.Modules.GoTo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.GoTo.Abstractions
{
    /// <summary>
    /// Handles comparison of data.
    /// </summary>
    public interface IHCGoToService
    {
        /// <summary>
        /// Get available resolver definitions.
        /// </summary>
        List<HCGoToResolverDefinition> GetResolvers();

        /// <summary>
        /// Attempt to locate matching instances.
        /// </summary>
        Task<List<HCGoToResolvedDataWithResolverId>> TryLocateInstance(string[] handlerIds, string input);
    }
}
