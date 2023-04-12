#if NETFULL
using System.CodeDom.Compiler;
#endif

namespace QoDL.Toolkit.Module.DynamicCodeExecution.Abstractions;

/// <summary>
/// Processes source code before it is executed.
/// </summary>
public interface IDynamicCodePreProcessor
{
    /// <summary>
    /// Id of the pre-processor used to disable it from the code.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Optional title returned in the options model.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Optional description returned in the options model.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Allow the user to disable the pre-processor from the code?
    /// </summary>
    bool CanBeDisabled { get; set; }

#if NETFULL
    /// <summary>
    /// Processes source code before it is executed.
    /// </summary>
    /// <param name="options">Compiler options that will be used.</param>
    /// <param name="code">The source code to be executed.</param>
    string PreProcess(CompilerParameters options, string code);
#endif
}
