namespace HealthCheck.Module.DynamicCodeExecution.Models
{
    /// <summary>
    /// Represents a diff of two data dumps.
    /// </summary>
    public class DiffModel
    {
        /// <summary>
        /// Title of the diff.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Left side of the diff.
        /// </summary>
        public DataDump Left { get; set; }

        /// <summary>
        /// Right side of the diff.
        /// </summary>
        public DataDump Right { get; set; }
    }
}
