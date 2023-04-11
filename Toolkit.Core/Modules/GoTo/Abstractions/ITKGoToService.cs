using QoDL.Toolkit.Core.Modules.GoTo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.GoTo.Abstractions
{
    /// <summary>
    /// Handles comparison of data.
    /// </summary>
    public interface ITKGoToService
    {
        /// <summary>
        /// Get available resolver definitions.
        /// </summary>
        List<TKGoToResolverDefinition> GetResolvers();

        /// <summary>
        /// Attempt to locate matching instances.
        /// </summary>
        Task<List<TKGoToResolvedDataWithResolverId>> TryLocateInstance(string[] handlerIds, string input);
    }
}
