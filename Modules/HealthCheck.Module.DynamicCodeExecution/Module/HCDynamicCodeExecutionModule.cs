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
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => CreateFrontendOptionsObject();

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCDynamicCodeExecutionModuleConfig();

        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,

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
        /// <param name="context"></param>
        /// <param name="source">Model with C# code to execute.</param>
        /// <returns>A <see cref="DynamicCodeExecutionResultModel"/></returns>
        [HealthCheckModuleMethod(AccessOption.ExecuteCustomScript)]
        public DynamicCodeExecutionResultModel ExecuteCode(HealthCheckModuleContext context, DynamicCodeExecutionSourceModel source)
        {
            var result = new DynamicCodeExecutionResultModel()
            {
                Code = source.Code,
            };

            var allowedResult = AllowExecution(source.Code);
            if (!allowedResult.IsAllowed)
            {
                result.Message = allowedResult.Message;
                context.AddAuditEvent("ExecuteCode", $"Was denied with the message '{allowedResult.Message}'");
                return result;
            }
            else
            {
                context.AddAuditEvent("ExecuteCode", $"Executed code.")
                    .AddClientConnectionDetails(context)
                    .AddBlob("Code", source.Code, onlyIfThisIsTrue: Options.StoreCopyOfExecutedScriptsAsAuditBlobs);
#if NETFULL || NETCORE
                var executor = CreateExecutor();

                result.Success = true;
                result.Message = "OK";
                result.CodeExecutionResult = executor.ExecuteCode(source.Code, source.DisabledPreProcessorIds);
                result.Code = result.CodeExecutionResult.Code;
                return result;
#else
                result.Message = "Can't execute code in .net standard mode. Need to reference .NET 6/framework/core.";
                return result;
#endif
            }
        }

        /// <summary>
        /// Executes a script that has been stored on the server.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ExecuteSavedScript)]
        public async Task<DynamicCodeExecutionResultModel> ExecuteScriptById(HealthCheckModuleContext context, Guid id)
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

            context.AddAuditEvent("ExecuteScriptById", $"Executing script with id '{id}'.");
            return ExecuteCode(context, source);
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
        public async Task<bool> DeleteScript(HealthCheckModuleContext context, Guid id)
        {
            var success = (await this.Options.ScriptStorage?.DeleteScript(id));
            context.AddAuditEvent("DeleteScript", (success) 
                ? $"Deleted script with id '{id}'." 
                : $"Failed to delete script with id '{id}'.");
            return success;
        }

        /// <summary>
        /// Creates a new script.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.CreateNewScriptOnServer)]
        public async Task<DynamicCodeScript> AddNewScript(HealthCheckModuleContext context, DynamicCodeScript script)
        {
            if (this.Options.ScriptStorage == null || script == null) return null;

            var existingScript = await this.Options.ScriptStorage.GetScript(script.Id);
            if (existingScript != null) return null;

            var createdScript = await this.Options.ScriptStorage?.SaveScript(script);
            context.AddAuditEvent("AddNewScript", $"Added new script with id '{createdScript?.Id}'.");
            return createdScript;
        }

        /// <summary>
        /// Edit an existing script.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.EditExistingScriptOnServer)]
        public async Task<DynamicCodeScript> SaveScriptChanges(HealthCheckModuleContext context, DynamicCodeScript script)
        {
            if (this.Options.ScriptStorage == null || script == null) return null;

            var existingScript = await this.Options.ScriptStorage.GetScript(script.Id);
            if (existingScript == null) return null;

            context.AddAuditEvent("SaveScriptChanges", $"Saved changes to script with id '{script?.Id}'.");
            return await this.Options.ScriptStorage.SaveScript(script);
        }

        /// <summary>
        /// Attempt to auto-complete code.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ExecuteCustomScript)]
        public async Task<IEnumerable<IDynamicCodeCompletionData>> AutoComplete(CompletionRequest request)
        {
            if (this.Options.AutoCompleter == null || request == null) return null;
            
#if NETFULL || NETCORE
            var assemblyLocations = CreateExecutor().GetAssemblyLocations();
            return await Options.AutoCompleter.GetAutoCompleteSuggestionsAsync(request.Code, assemblyLocations?.ToArray(), request.Position);
#else
            return await Task.FromResult(new List<IDynamicCodeCompletionData>());
#endif
        }
        #endregion

        #region Private helpers
        private DynamicCodeExecutionModuleFrontendOptionsModel CreateFrontendOptionsObject()
        {
            return new DynamicCodeExecutionModuleFrontendOptionsModel()
            {
                PreProcessors = (Options.PreProcessors ?? Enumerable.Empty<IDynamicCodePreProcessor>()).Select(x => new PreProcessorMetadata()
                {
                    Id = x.Id,
                    Name = x.Name ?? x.Id,
                    Description = x.Description,
                    CanBeDisabled = x.CanBeDisabled
                }),
                ServerSideScriptsEnabled = Options.ScriptStorage != null,
                AutoCompleteEnabled = Options.AutoCompleter != null,
                DefaultScript = GetDefaultScript(),
                StaticSnippets = Options.StaticSnippets ?? new List<CodeSuggestion>()
            };
        }

        private string GetDefaultScript()
        {
            var usings = string.Join("\n", (Options.AdditionalUsings ?? new List<string>()).Select(x => $"using {x};"));
            var script = Options.DefaultScript;
            script = script.Replace("[[AdditionalUsings]]", usings);
            return script;
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

        #if NETFULL || NETCORE
        private RuntimeCodeTester CreateExecutor()
        {
            var executor = new RuntimeCodeTester(new RuntimeCodeTesterConfig()
                {
                    PreProcessors = Options.PreProcessors,
                    AdditionalReferencedAssemblies = Options.AdditionalReferencedAssemblies
                },
                Options.TargetAssembly);

            return executor;
        }
        #endif
        #endregion
    }
}
