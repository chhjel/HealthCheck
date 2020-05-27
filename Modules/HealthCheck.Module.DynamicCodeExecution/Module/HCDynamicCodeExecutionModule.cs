using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using HealthCheck.Module.DynamicCodeExecution.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Module.DynamicCodeExecution.Module
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

            /// <summary>
            /// Allow writing and executing scripts.
            /// </summary>
            ExecuteCustomScript = 1,

            /// <summary>
            /// Allow executing scripts that are already saved on the server.
            /// </summary>
            ExecuteSavedScript = 2,

            /// <summary>
            /// Allow loading scripts from server.
            /// </summary>
            LoadScriptFromServer = 4,

            /// <summary>
            /// Allow saving new scripts to server.
            /// </summary>
            CreateNewScriptOnServer = 8,

            /// <summary>
            /// Allow saving over existing scripts on server.
            /// </summary>
            EditExistingScriptOnServer = 16,

            /// <summary>
            /// Allow deleting existing scripts on server.
            /// </summary>
            DeleteExistingScriptOnServer = 32
        }

        #region Invokable methods
        /// <summary>
        /// Executes the provided C# code.
        /// </summary>
        /// <param name="source">Model with C# code to execute.</param>
        /// <returns>A <see cref="DynamicCodeExecutionResultModel"/></returns>
        [HealthCheckModuleMethod(AccessOption.ExecuteCustomScript)]
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
#if NETFULL
                var executor = CreateExecutor();

                result.Success = true;
                result.Message = "OK";
                result.CodeExecutionResult = executor.ExecuteCode(source.Code, source.DisabledPreProcessorIds);
                result.Code = result.CodeExecutionResult.Code;
                return result;
#else
                result.Message = "Can't execute code in .net standard mode. Need to reference the .net framework.";
                return result;
#endif
            }
        }

        /// <summary>
        /// Executes a script that has been stored on the server.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ExecuteSavedScript)]
        public async Task<DynamicCodeExecutionResultModel> ExecuteScriptById(Guid id)
        {
            var script = await Options.ScriptStorage?.GetScript(id);
            if (script == null)
            {
                return new DynamicCodeExecutionResultModel()
                {
                    Success = false,
                    Message = $"Script with id {id} was not found."
                };
            }

            var source = new DynamicCodeExecutionSourceModel()
            {
                Code = script.Code,
                DisabledPreProcessorIds = new List<string>()
            };

            return ExecuteCode(source);
        }

        /// <summary>
        /// Get all stored scripts.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.LoadScriptFromServer)]
        public async Task<List<DynamicCodeScript>> GetScripts()
            => (this.Options.ScriptStorage == null)
            ? new List<DynamicCodeScript>()
            : (await this.Options.ScriptStorage.GetAllScripts());

        /// <summary>
        /// Deletes a single stored script.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.DeleteExistingScriptOnServer)]
        public async Task<bool> DeleteScript(Guid id) => (await this.Options.ScriptStorage?.DeleteScript(id)) == true;

        /// <summary>
        /// Creates a new script.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.CreateNewScriptOnServer)]
        public async Task<DynamicCodeScript> AddNewScript(DynamicCodeScript script)
        {
            if (this.Options.ScriptStorage == null || script == null) return null;

            var existingScript = await this.Options.ScriptStorage.GetScript(script.Id);
            if (existingScript != null) return null;

            return await this.Options.ScriptStorage?.SaveScript(script);
        }

        /// <summary>
        /// Edit an existing script.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.EditExistingScriptOnServer)]
        public async Task<DynamicCodeScript> SaveScriptChanges(DynamicCodeScript script)
        {
            if (this.Options.ScriptStorage == null || script == null) return null;

            var existingScript = await this.Options.ScriptStorage.GetScript(script.Id);
            if (existingScript == null) return null;

            return await this.Options.ScriptStorage.SaveScript(script);
        }
        #endregion

        #region Private helpers
        private DCEModuleFrontendOptionsModel CreateFrontendOptionsObject()
        {
            return new DCEModuleFrontendOptionsModel()
            {
                PreProcessors = (Options.PreProcessors ?? Enumerable.Empty<IDynamicCodePreProcessor>()).Select(x => new PreProcessorMetadata()
                {
                    Id = x.Id,
                    Name = x.Name ?? x.Id,
                    Description = x.Description,
                    CanBeDisabled = x.CanBeDisabled
                }),
                ServerSideScriptsEnabled = Options.ScriptStorage != null,
                DefaultScript = Options.DefaultScript
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

        #if NETFULL
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
        #endif
        #endregion
    }
}
