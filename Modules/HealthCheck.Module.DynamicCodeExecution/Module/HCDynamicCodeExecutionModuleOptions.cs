using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using HealthCheck.Module.DynamicCodeExecution.Models;
using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Module.DynamicCodeExecution.Module
{
    /// <summary>
    /// Options for <see cref="HCDynamicCodeExecutionModule"/>.
    /// </summary>
    public class HCDynamicCodeExecutionModuleOptions
    {
        /// <summary>
        /// Optional list of pre-processors to run the code through before execution.
        /// </summary>
        public IEnumerable<IDynamicCodePreProcessor> PreProcessors { get; set; }

        /// <summary>
        /// A collection of static suggestions triggered by entering "@@@.".
        /// </summary>
        public IEnumerable<CodeSuggestion> StaticSnippets { get; set; }

        /// <summary>
        /// Optional list of code validators to run the code through before execution, and optionally halt execution.
        /// </summary>
        public IEnumerable<IDynamicCodeValidator> Validators { get; set; }

        /// <summary>
        /// Optional serverside storage of scripts.
        /// </summary>
        public IDynamicCodeScriptStorage ScriptStorage { get; set; }

        /// <summary>
        /// Optional provider for autocompletion of code.
        /// </summary>
        public IDynamicCodeAutoCompleter AutoCompleter { get; set; }

        /// <summary>
        /// Code used for new scripts.
        /// </summary>
        public string DefaultScript { get; set; }

        /// <summary>
        /// Your entry assembly.
        /// </summary>
        public Assembly TargetAssembly { get; set; }

        /// <summary>
        /// Any additional assemblies to reference in addition to all assemblies referenced by the target assembly.
        /// <para>Defaults to only netstandard.dll</para>
        /// </summary>
        public List<string> AdditionalReferencedAssemblies { get; set; } = new List<string>
        {
            "netstandard.dll"
        };
    }
}
