using HealthCheck.Module.DynamicCodeExecution.Models;
using System.Threading.Tasks;

namespace HealthCheck.Module.DynamicCodeExecution.Abstractions
{
    /// <summary>
    /// Saves/loads scripts that can be executed.
    /// </summary>
    public interface IDynamicCodeScriptStorage
    {
        /// <summary>
        /// Get all stored scripts.
        /// </summary>
        Task<DynamicCodeScript> GetAllScripts();

        /// <summary>
        /// Save the given script and return the updated script.
        /// </summary>
        DynamicCodeScript SaveScript(DynamicCodeScript script);
    }
}
