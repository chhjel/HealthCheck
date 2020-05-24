namespace HealthCheck.Module.DynamicCodeExecution.Models
{
    /// <summary>
    /// A script that was saved.
    /// </summary>
    public class DynamicCodeScript
    {
        /// <summary>
        /// Unique id of this script.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Title of the script.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description of the script.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The code itself.
        /// </summary>
        public string Code { get; }
    }
}
