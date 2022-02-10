#if NETFULL
using HealthCheck.Core.Util;
using HealthCheck.Module.DynamicCodeExecution.Exceptions;
using HealthCheck.Module.DynamicCodeExecution.Models;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HealthCheck.Module.DynamicCodeExecution
{
    /// <summary>
    /// Compiles and executes code.
    /// </summary>
    internal class RuntimeCodeTester
    {
        /// <summary>
        /// Configuration for the code execution. Authorization, preprocessors etc.
        /// </summary>
        public RuntimeCodeTesterConfig Config { get; set; }

        /// <summary>
        /// Your entry assembly.
        /// </summary>
        public Assembly TargetAssembly { get; set; }

        /// <summary>
        /// Extra references.
        /// </summary>
        private List<string> AdditionalReferencedAssemblies { get; set; }

        /// <summary>
        /// Create a new instance of the runtime code tester.
        /// </summary>
        /// <param name="config">Various configs</param>
        /// <param name="assembly">Your entry assembly.</param>
        public RuntimeCodeTester(RuntimeCodeTesterConfig config, Assembly assembly)
        {
            Config = config ?? new RuntimeCodeTesterConfig();
            TargetAssembly = assembly ?? Assembly.GetEntryAssembly();
            AdditionalReferencedAssemblies = config?.AdditionalReferencedAssemblies;
        }

        /// <summary>
        /// Applies any pre-processors and executes the code.
        /// </summary>
        /// <param name="source">C# to execute.</param>
        /// <param name="disabledPreProcessorIds">List of pre-processor ids that will be requested disabled.</param>
        /// <returns>A result with any output, errors and dumps.</returns>
        public CodeExecutionResult ExecuteCode(string source, List<string> disabledPreProcessorIds = null)
        {
            disabledPreProcessorIds ??= new List<string>();
            var options = CreateCompilerParameters();
            var appliedPreProcessorIds = new List<string>();
            try
            {
                source = PreProcess(source, options, disabledPreProcessorIds, appliedPreProcessorIds);
            }
            catch (Exception ex)
            {
                return new CodeExecutionResult()
                {
                    Status = CodeExecutionResult.StatusTypes.CompilerError,
                    Errors = ExceptionToCodeError(ex)
                };
            }

            return ExecuteCode(source, options, appliedPreProcessorIds);
        }

        private CompilerParameters CreateCompilerParameters()
        {
            var options = new CompilerParameters();
            AddAssemblies(Assembly.GetExecutingAssembly(), options);
            AddAssemblies(TargetAssembly, options);

            if (AdditionalReferencedAssemblies != null)
            {
                foreach (var reference in AdditionalReferencedAssemblies)
                {
                    options.ReferencedAssemblies.Add(reference);
                }
            }

            return options;
        }

        /// <summary>
        /// Get a list of all the assemblies used.
        /// </summary>
        public IEnumerable<string> GetAssemblyLocations()
        {
            var list = new List<string>();
            var exec = Assembly.GetExecutingAssembly();
            if (exec != null)
            {
                list.Add(exec.Location);
                list.AddRange(exec.GetReferencedAssemblies().Select(x => ReflectionOnlyLoadWithPolicy(x.FullName).Location));
            }
            if (TargetAssembly != null)
            {
                list.Add(TargetAssembly.Location);
                list.AddRange(TargetAssembly.GetReferencedAssemblies().Select(x => ReflectionOnlyLoadWithPolicy(x.FullName).Location));
            }
            return list.GroupBy(x => x).Select(x => x.First());
        }

        private string PreProcess(string source, CompilerParameters options, List<string> disabledPreProcessorIds, List<string> appliedPreProcessorIds)
        {
            if (Config.PreProcessors == null || !Config.PreProcessors.Any())
            {
                return source;
            }

            foreach(var processor in Config.PreProcessors.Where(p => p != null && (!p.CanBeDisabled || !disabledPreProcessorIds.Contains(p.Id))))
            {
                try
                {
                    appliedPreProcessorIds.Add(processor.Id);
                    source = processor.PreProcess(options, source);
                }
                catch (Exception ex)
                {
                    throw new PreProcessorException($"The preprocessor '{processor.GetType().Name}' caused an error.", ex);
                }
            }

            return source;
        }

        private CodeExecutionResult ExecuteCode(string source, CompilerParameters options, List<string> appliedPreProcessorIds)
        {
            var provider = CodeDomProvider.CreateProvider("C#");

            var result = new CodeExecutionResult()
            {
                Code = source,
                AppliedPreProcessorIds = appliedPreProcessorIds
            };
            var compilerResult = provider.CompileAssemblyFromSource(options, source, ExtraSource);
            if (compilerResult.Errors.Count > 0)
            {
                var errors = new List<CodeError>();
                foreach (CompilerError err in compilerResult.Errors)
                {
                    errors.Add(new CodeError(err.Line, err.Column, err.ErrorText));
                }
                result.Errors = errors;
                result.Status = CodeExecutionResult.StatusTypes.CompilerError;
                return result;
            }

            // Check that the required method that we are going to call is in place.
            Type type = null;
            object instance = null;
            MethodInfo method = null;
            var dumps = new List<DataDump>();
            var diffs = new List<DiffModel>();
            try
            {
                // Init internal
                compilerResult.CompiledAssembly
                    .GetType("DCEInternalHelpers")
                    .GetMethod("Init", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, new object[] { dumps, diffs });

                // Invoke custom main method
                type = compilerResult.CompiledAssembly.GetType("CodeTesting.EntryClass");
                instance = HCIoCUtils.GetInstanceExt(type);
                method = type.GetMethod("Main");
            }
            catch (Exception ex)
            {
                result.Status = CodeExecutionResult.StatusTypes.RuntimeError;
                result.Errors.Add(new CodeError(
                    $"Namespace must be CodeTesting, classname must be EntryClass, and method must be Main and optionally return something stringable. Exception was: {ex}"
                ));
                return result;
            }

            if (method == null)
            {
                result.Status = CodeExecutionResult.StatusTypes.RuntimeError;
                result.Errors.Add(new CodeError("No Main method was found. It has to exist within CodeTesting.EntryClass, can be async and optionally return something stringable."));
                return result;
            }

            try
            {
                var returnType = method.ReturnType;
                object rawResult = null;

                // Task or Task<T>
                if (returnType == typeof(Task)
                    || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)))
                {
                    rawResult = HCAsyncUtils.InvokeAsyncSync(method, instance, null);
                    if (returnType == typeof(Task))
                    {
                        rawResult = null;
                    }
                }
                // Not async
                else
                {
                    rawResult = method.Invoke(instance, null);
                }

                result.Output = rawResult?.ToString();
                result.Status = CodeExecutionResult.StatusTypes.Executed;
            }
            catch (Exception ex)
            {
                result.Status = CodeExecutionResult.StatusTypes.RuntimeError;
                result.Errors = ExceptionToCodeError(ex);
            }

            result.Dumps = dumps.Where(d => d.Display).ToList();
            result.Diffs = diffs;
            return result;
        }

        private void AddAssemblies(Assembly currentAssembly, CompilerParameters options)
        {
            options.ReferencedAssemblies.Add(currentAssembly.Location);

            foreach(var refAssemblyName in currentAssembly.GetReferencedAssemblies())
            {
                AddAssemblyWithPolicy(refAssemblyName.FullName, options);
            }
        }

        private void AddAssemblyWithPolicy(string assemblyName, CompilerParameters options)
        {
            var refAssembly = ReflectionOnlyLoadWithPolicy(assemblyName);
            options.ReferencedAssemblies.Add(refAssembly.Location);
        }

        private Assembly ReflectionOnlyLoadWithPolicy(string assemblyName)
        {
            var newName = AppDomain.CurrentDomain?.ApplyPolicy(assemblyName);
            return Assembly.ReflectionOnlyLoad(newName);
        }

        private List<CodeError> ExceptionToCodeError(Exception ex)
        {
            var errors = new List<CodeError>
            {
                new CodeError($"{ex}{Environment.NewLine}{ex.StackTrace}")
            };

            if (ex.InnerException != null)
            {
                errors.AddRange(ExceptionToCodeError(ex.InnerException));
            }
            return errors;
        }

        private readonly string ExtraSource = $@"
    public static class DCEInternalHelpers 
    {{
        public static System.Collections.Generic.List<HealthCheck.Module.DynamicCodeExecution.Models.DataDump> Dumps {{ get; set; }}
        public static System.Collections.Generic.List<HealthCheck.Module.DynamicCodeExecution.Models.DiffModel> Diffs {{ get; set; }}

        public static void Init(System.Collections.Generic.List<HealthCheck.Module.DynamicCodeExecution.Models.DataDump> dumpList, 
                                System.Collections.Generic.List<HealthCheck.Module.DynamicCodeExecution.Models.DiffModel> diffList) 
        {{
            Dumps = dumpList;
            Diffs = diffList;
        }}
    }}

    public static class DCEUtils
    {{
        public static void SaveDumps(string pathOrFilename = null, bool includeTitle = false, bool includeType = false)
        {{ HealthCheck.Module.DynamicCodeExecution.Util.DumpHelper.SaveDumps(DCEInternalHelpers.Dumps, pathOrFilename, includeTitle, includeType); }}
    }}

	public static class DCEExtensions
    {{
		public static T Dump<T>(this T obj, string title = null, bool display = true, bool ignoreErrors = true)
        {{ return HealthCheck.Module.DynamicCodeExecution.Util.DumpHelper.Dump<T>(obj, DCEInternalHelpers.Dumps, title, display, ignoreErrors); }}
		
		public static T Dump<T>(this T obj, string title = null, bool display = true, params Newtonsoft.Json.JsonConverter[] converters)
        {{ return HealthCheck.Module.DynamicCodeExecution.Util.DumpHelper.Dump<T>(obj, DCEInternalHelpers.Dumps, title, display, converters); }}
	
		public static T Dump<T>(this T obj, System.Func<T, string> dumpConverter, string title = null, bool display = true)
        {{ return HealthCheck.Module.DynamicCodeExecution.Util.DumpHelper.Dump<T>(obj, DCEInternalHelpers.Dumps, dumpConverter, title, display); }}

        public static TLeft Diff<TLeft, TRight>(this TLeft left, TRight right, bool onlyIfDifferent = true, string title = null, bool ignoreErrors = true)
        {{ return HealthCheck.Module.DynamicCodeExecution.Util.DumpHelper.Diff<TLeft, TRight>(left, right, onlyIfDifferent, DCEInternalHelpers.Diffs, title, ignoreErrors); }}
	}}
";
    }
}
#endif
