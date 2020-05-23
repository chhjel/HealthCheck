using HealthCheck.DynamicCodeExecution.Abstractions;
using System;
using System.CodeDom.Compiler;

namespace HealthCheck.DynamicCodeExecution.PreProcessors
{
    /// <summary>
    /// Pre-processes the code using a custom func method.
    /// </summary>
    public class FuncPreProcessor : IDynamicCodePreProcessor
    {
        /// <summary>
        /// Id of the pre-processor used to disable it from the code.
        /// </summary>
        public string Id { get; set; } = "Func";

        /// <summary>
        /// Optional title returned in the options model.
        /// </summary>
        public string Name { get; set; } = "Unnamed FuncPreProcessor";

        /// <summary>
        /// Optional description returned in the options model.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Allow this pre-processor to be disabled by the user?
        /// </summary>
        public bool CanBeDisabled { get; set; } = true;

        /// <summary>
        /// Custom function that processes the code
        /// </summary>
        public Func<CompilerParameters, string, string> CustomFunc { get; set; }

        /// <summary>
        /// Pre-processes the code using a custom func method.
        /// </summary>
        /// <param name="customFunc">Custom function that processes the code.</param>
        public FuncPreProcessor(Func<CompilerParameters, string, string> customFunc)
        {
            CustomFunc = customFunc;
        }

        /// <summary>
        /// Wraps the first batch of using statements found in a region.
        /// </summary>
        public string PreProcess(CompilerParameters options, string code)
        {
            return CustomFunc(options, code ?? String.Empty);
        }

    }
}
