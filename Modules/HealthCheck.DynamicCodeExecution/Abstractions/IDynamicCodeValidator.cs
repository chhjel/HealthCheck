using HealthCheck.DynamicCodeExecution.Models;

namespace HealthCheck.DynamicCodeExecution.Abstractions
{
    /// <summary>
    /// Implement to check and prevent the code from being executed.
    /// </summary>
    public interface IDynamicCodeValidator
    {
        DynamicCodeValidationResult Validate(string code);
    }
}
