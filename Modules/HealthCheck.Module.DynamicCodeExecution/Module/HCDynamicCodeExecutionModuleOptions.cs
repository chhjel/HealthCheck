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

        /// <summary>
        /// Code used for new scripts.
        /// <para>Can contain [[AdditionalUsings]] where <see cref="AdditionalUsings"/> will be inserted.</para>
        /// </summary>
        public string DefaultScript { get; set; } = DEFAULT_SCRIPT;

        /// <summary>
        /// List of namespaces to add usings for in the default script.
        /// </summary>
        public List<string> AdditionalUsings { get; set; } = new List<string>
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.Text.RegularExpressions",
            "System.Collections.Generic"
        };

        private const string DEFAULT_SCRIPT = @"// Title: 
#region Usings
[[AdditionalUsings]]
#endregion

namespace CodeTesting 
{
    public class EntryClass
    {
        public void Main() 
        {
            new { Hello = ""World"" }.Dump();
        }

        #region Docs
        // To dump a value use the extension methods .Dump():
        // * T Dump<T>(this T obj, string title = null, bool display = true, bool ignoreErrors = true)
        // * T Dump<T>(this T obj, string title = null, bool display = true, params JsonConverter[] converters)
        // * T Dump<T>(this T obj, Func<T, string> dumpConverter, string title = null, bool display = true)

        // To save all dumped values to a file:
        // * DCEUtils.SaveDumps(string pathOrFilename = null, bool includeTitle = false, bool includeType = false)
        //   Default filename is %tempfolder%\\DCEDump_{date}.txt
        #endregion
    }
}";
    }
}
