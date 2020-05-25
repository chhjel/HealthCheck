using System;

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
        public Guid Id { get; set; }

        /// <summary>
        /// Title of the script.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Script content.
        /// </summary>
        public string Code { get; set; }
    }
}
