#if NETFULL
using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using System;
using System.CodeDom.Compiler;
using System.Text;

namespace HealthCheck.Module.DynamicCodeExecution.PreProcessors
{
    /// <summary>
    /// Pre-processes the code using a custom func method.
    /// </summary>
    public class FuncPreProcessor : IDynamicCodePreProcessor
    {
        /// <summary>
        /// Id of the pre-processor used to disable it from the code.
        /// <para>Defaults to base64 of the name.</para>
        /// </summary>
        public string Id { get; set; }

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
        /// <param name="name">Name to show in the UI.</param>
        /// <param name="description">Optional description to show in the UI.</param>
        /// <param name="canBeDisabled">If set to false, the option to disable this pre-processor by the user will be disabled.</param>
        public FuncPreProcessor(string name, Func<CompilerParameters, string, string> customFunc,
            string description = null, bool canBeDisabled = true)
        {
            CustomFunc = customFunc;
            Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(name));
            Name = name;
            Description = description;
            CanBeDisabled = canBeDisabled;
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
#endif
