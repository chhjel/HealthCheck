using HealthCheck.Module.DynamicCodeExecution.Abstractions;
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
        /// Optional list of code validators to run the code through before execution, and optionally halt execution.
        /// </summary>
        public IEnumerable<IDynamicCodeValidator> Validators { get; set; }

        /// <summary>
        /// Optional serverside storage of scripts.
        /// </summary>
        public IDynamicCodeScriptStorage ScriptStorage { get; set; }

        /// <summary>
        /// Your entry assembly.
        /// </summary>
        public Assembly TargetAssembly { get; set; }
    }
}
