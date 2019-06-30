namespace HealthCheck.Core.Exceptions
{
    /// <summary>
    /// Thrown when a test method has a wrong signature.
    /// </summary>
    public class InvalidTestDefinitionException : System.Exception
    {
        /// <summary>
        /// Create a new <see cref="InvalidTestDefinitionException"/>.
        /// </summary>
        public InvalidTestDefinitionException(string message) : base(message) {}
    }
}
