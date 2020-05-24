namespace HealthCheck.Module.DynamicCodeExecution.Models
{
    /// <summary>
    /// Result model from the module.
    /// </summary>
    public class DynamicCodeExecutionResultModel
    {
        /// <summary>
        /// True if the user was allowed to execute code.
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Message telling the user if it was allowed to execute code or not.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Source code that was given by user, processed by any pre-processors.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The result of the execution or null if the user was not allowed to execute code.
        /// </summary>
        public CodeExecutionResult CodeExecutionResult { get; set; } = new CodeExecutionResult();
    }
}
