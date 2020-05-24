using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Module.DynamicCodeExecution.Models
{
    /// <summary>
    /// Options used by DCE module.
    /// </summary>
    internal class OptionsModel
    {
        /// <summary>
        /// List of pre-processors to disable.
        /// </summary>
        public IEnumerable<PreProcessorMetadata> PreProcessors { get; set; } = Enumerable.Empty<PreProcessorMetadata>();
    }

    /// <summary>
    /// Information about a pre-processor.
    /// </summary>
    internal class PreProcessorMetadata
    {
        /// <summary>
        /// Id of the pre-processor.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the pre-processor.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Can the user disable it?
        /// </summary>
        public bool CanBeDisabled { get; set; }
    }
}
