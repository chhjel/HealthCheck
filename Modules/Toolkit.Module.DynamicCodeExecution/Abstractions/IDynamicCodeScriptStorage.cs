using QoDL.Toolkit.Module.DynamicCodeExecution.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DynamicCodeExecution.Abstractions;

/// <summary>
/// Saves/loads scripts that can be executed.
/// </summary>
public interface IDynamicCodeScriptStorage
{
    /// <summary>
    /// Get all stored scripts.
    /// </summary>
    Task<List<DynamicCodeScript>> GetAllScripts();

    /// <summary>
    /// Get a single stored script.
    /// </summary>
    Task<DynamicCodeScript> GetScript(Guid id);

    /// <summary>
    /// Deletes a single stored script.
    /// </summary>
    Task<bool> DeleteScript(Guid id);

    /// <summary>
    /// Save or create the given script and return the script with any changes.
    /// </summary>
    Task<DynamicCodeScript> SaveScript(DynamicCodeScript script);
}
