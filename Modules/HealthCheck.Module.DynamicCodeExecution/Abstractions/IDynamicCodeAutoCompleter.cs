using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Module.DynamicCodeExecution.Abstractions
{
    /// <summary>
    /// Gives suggestions while writing code.
    /// </summary>
    public interface IDynamicCodeAutoCompleter
    {
        /// <summary>
        /// Get suggestions.
        /// </summary>
        /// <param name="code">The whole code</param>
        /// <param name="assemblyLocations">Location of assemblies to check</param>
        /// <param name="position">Cursor position</param>
        Task<IEnumerable<IDynamicCodeCompletionData>> GetAutoCompleteSuggestionsAsync(string code, string[] assemblyLocations, int position);
    }
}
