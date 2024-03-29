using QoDL.Toolkit.Module.DynamicCodeExecution.Models;

namespace QoDL.Toolkit.Module.DynamicCodeExecution.Abstractions;

/// <summary>
/// Implement to check and prevent the code from being executed.
/// </summary>
public interface IDynamicCodeValidator
{
    /// <summary>
    /// Checks if the given code is allowed to be executed.
    /// </summary>
    DynamicCodeValidationResult Validate(string code);
}
