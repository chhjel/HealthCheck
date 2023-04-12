using QoDL.Toolkit.Module.DynamicCodeExecution.Abstractions;
using QoDL.Toolkit.Module.DynamicCodeExecution.Models;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Module.DynamicCodeExecution.Module;

/// <summary>
/// Options for <see cref="TKDynamicCodeExecutionModule"/>.
/// </summary>
public class TKDynamicCodeExecutionModuleOptions
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
        "System.Collections.Generic",
        "QoDL.Toolkit.WebUI.Util",
        "QoDL.Toolkit.Core.Util",
        "System.Threading.Tasks"
    };

    /// <summary>
    /// If enabled any executed scripts will be save a copy of the script into the audit blob storage.
    /// </summary>
    public bool StoreCopyOfExecutedScriptsAsAuditBlobs { get; set; }

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
        // Main method can be async Task/Task<T>, void or directly return something stringable.
        // Constructor parameter injection in EntryClass is supported.

        // To dump a value use the extension methods .Dump():
        // * T Dump<T>(this T obj, string title = null, bool display = true, bool ignoreErrors = true)
        // * T Dump<T>(this T obj, string title = null, bool display = true, params JsonConverter[] converters)
        // * T Dump<T>(this T obj, Func<T, string> dumpConverter, string title = null, bool display = true)

        // To save all dumped values to a file:
        // * DCEUtils.SaveDumps(string pathOrFilename = null, bool includeTitle = false, bool includeType = false)
        //   Default filename is %tempfolder%\\DCEDump_{date}.txt

        // Utils for quickly instancing and invoking:
        // GetInstance retrieves from IoC or instantiate new, with recursive parameter override by type.
        // * T TKIoCUtils.GetInstance<T>(params object[] forcedParameterValues)
        // * TResult TKReflectionUtils.TryInvokeMethod<TClass, TResult>(string methodName, params object[] parameters)
        // * object TKReflectionUtils.TryInvokeMethod<TClass>(string methodName, params object[] parameters)
        
        // Async utils:
        // * TResult TKAsyncUtils.RunSync<TResult>(Func<Task<TResult>> func)
        // * void TKAsyncUtils.RunSync(Func<Task> func)
        // * async Task<T> TKAsyncUtils.InvokeAsync<T>(MethodInfo method, object obj, params object[] parameters)
        // * async Task<object> TKAsyncUtils.InvokeAsync(MethodInfo method, object obj, params object[] parameters)
        // * T TKAsyncUtils.InvokeAsyncSync<T>(MethodInfo method, object obj, params object[] parameters)
        // * object TKAsyncUtils.InvokeAsyncSync(MethodInfo method, object obj, params object[] parameters)
        #endregion
    }
}";
}
