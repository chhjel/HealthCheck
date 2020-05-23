using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.DynamicCodeExecution;
using HealthCheck.DynamicCodeExecution.Abstractions;
using HealthCheck.DynamicCodeExecution.Models;
using System;
using System.Linq;

namespace HealthCheck.Core.Modules.Settings
{
    /// <summary>
    /// Module for compiling and executing code at runtime.
    /// </summary>
    public class HCDynamicCodeExecutionModule : HealthCheckModuleBase<HCDynamicCodeExecutionModule.AccessOption>
    {
        private HCDynamicCodeExecutionModuleOptions Options { get; }

        /// <summary>
        /// Module for compiling and executing code at runtime.
        /// </summary>
        public HCDynamicCodeExecutionModule(HCDynamicCodeExecutionModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(AccessOption access) => CreateFrontendOptionsObject();

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(AccessOption access) => new HCDynamicCodeExecutionModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            Nothing = 0,
        }

        #region Invokable methods
        /// <summary>
        /// Executes the provided C# code.
        /// </summary>
        /// <param name="source">Model with C# code to execute.</param>
        /// <returns>A <see cref="DynamicCodeExecutionResultModel"/></returns>
        [HealthCheckModuleMethod]
        public DynamicCodeExecutionResultModel ExecuteCode(DynamicCodeExecutionSourceModel source)
        {
            var result = new DynamicCodeExecutionResultModel()
            {
                Code = source.Code,
            };

            var allowedResult = AllowExecution(source.Code);
            if (!allowedResult.IsAllowed)
            {
                result.Message = allowedResult.Message;
                return result;
            }
            else
            {
                var executor = CreateExecutor();

                result.Success = true;
                result.Message = "OK";
                result.CodeExecutionResult = executor.ExecuteCode(source.Code, source.DisabledPreProcessorIds);
                result.Code = result.CodeExecutionResult.Code;
                return result;
            }
        }
        #endregion

        #region Private helpers
        private OptionsModel CreateFrontendOptionsObject()
        {
            return new OptionsModel()
            {
                PreProcessors = (Options.PreProcessors ?? Enumerable.Empty<IDynamicCodePreProcessor>()).Select(x => new PreProcessorMetadata()
                {
                    Id = x.Id,
                    Name = x.Name ?? x.Id,
                    Description = x.Description,
                    CanBeDisabled = x.CanBeDisabled
                })
            };
        }

        private DynamicCodeValidationResult AllowExecution(string code)
        {
            if (Options.Validators?.Any() != true)
            {
                return DynamicCodeValidationResult.Allow();
            }

            foreach(var validator in Options.Validators)
            {
                var result = validator.Validate(code);
                if (!result.IsAllowed)
                {
                    return result;
                }
            }

            return DynamicCodeValidationResult.Allow();
        }

        private RuntimeCodeTester CreateExecutor()
        {
            var executor = new RuntimeCodeTester(new RCTConfig()
                {
                    //AutoComplete = Options.AutoComplete,
                    PreProcessors = Options.PreProcessors
                },
                Options.TargetAssembly);

            return executor;
        }
        #endregion
    }
}
