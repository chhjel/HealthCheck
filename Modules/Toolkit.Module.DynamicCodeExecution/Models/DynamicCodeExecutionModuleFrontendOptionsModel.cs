using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Module.DynamicCodeExecution.Models
{
    /// <summary>
    /// Options used by DCE module.
    /// </summary>
    internal class DynamicCodeExecutionModuleFrontendOptionsModel
    {
        /// <summary>
        /// List of pre-processors to disable.
        /// </summary>
        public IEnumerable<PreProcessorMetadata> PreProcessors { get; set; } = Enumerable.Empty<PreProcessorMetadata>();

        /// <summary>
        /// True if script storage service is provided.
        /// </summary>
        public bool ServerSideScriptsEnabled { get; set; }

        /// <summary>
        /// True if autocompleter service is provided.
        /// </summary>
        public bool AutoCompleteEnabled { get; set; }

        /// <summary>
        /// A collection of static suggestions triggered by entering "@@@.".
        /// </summary>
        public IEnumerable<CodeSuggestion> StaticSnippets { get; set; }

        /// <summary>
        /// Code used for new scripts.
        /// </summary>
        public string DefaultScript { get; set; }
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
