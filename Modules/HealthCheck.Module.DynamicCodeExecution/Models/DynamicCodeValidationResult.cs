namespace HealthCheck.Module.DynamicCodeExecution.Models
{
    /// <summary></summary>
    public class DynamicCodeValidationResult
    {
        /// <summary>
        /// Set to false to disallow the exection of the code.
        /// </summary>
        public bool IsAllowed { get; set; }

        /// <summary>
        /// The message that will be displayed to the user if the code is not allowed to run.
        /// </summary>
        public string Message { get; set; }

        /// <summary></summary>
        /// <param name="allow">Set to false to disallow the exection of the code.</param>
        /// <param name="message">The message that will be displayed to the user if the code is not allowed to run.</param>
        public DynamicCodeValidationResult(bool allow, string message)
        {
            IsAllowed = allow;
            Message = message;
        }

        /// <summary>
        /// Create a result that denies the code execution with the given message.
        /// </summary>
        /// <param name="message">The message that will be displayed to the user if the code is not allowed to run.</param>
        public static DynamicCodeValidationResult Deny(string message) => new DynamicCodeValidationResult(false, message);

        /// <summary>
        /// Create a result that allows the code execution.
        /// </summary>
        public static DynamicCodeValidationResult Allow() => new DynamicCodeValidationResult(true, "OK");
    }
}
