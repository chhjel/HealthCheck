using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static HealthCheck.Core.Modules.Tests.Attributes.RuntimeTestParameterAttribute;

namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// A definition of a runtime test. Extracted from a method decorated with <see cref="RuntimeTestAttribute"/>.
    /// </summary>
    public class TestDefinition
    {
        /// <summary>
        /// Unique id for this test.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Name of the test. Defaults to the name of the method if no custom name is provided.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the test.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Test parameters.
        /// </summary>
        public TestParameter[] Parameters { get; private set; }

        /// <summary>
        /// Optional value to override default option on the test class.
        /// </summary>
        public bool? AllowParallelExecution { get; set; }

        /// <summary>
        /// If enabled the test in this class can be executed from the ui manually by default.
        /// </summary>
        public bool AllowManualExecution { get; set; } = true;

        /// <summary>
        /// Roles that can access this test method.
        /// </summary>
        public object RolesWithAccess { get; set; }

        /// <summary>
        /// Optional categories that can be filtered upon.
        /// </summary>
        public List<string> Categories { get; set; }

        /// <summary>
        /// Text on the button when the check is not executing.
        /// <para>Defaults to "Run"</para>
        /// </summary>
        public string RunButtonText { get; set; }

        /// <summary>
        /// Text on the button when the check is executing.
        /// <para>Defaults to "Runnings.."</para>
        /// </summary>
        public string RunningButtonText { get; set; }

        /// <summary>
        /// True if the test supports cancellation.
        /// </summary>
        public bool IsCancellable { get; set; }

        /// <summary>
        /// Test method.
        /// </summary>
        internal MethodInfo Method { get; private set; }

        /// <summary>
        /// Parent class definition;
        /// </summary>
        public TestClassDefinition ParentClass { get; private set; }

        /// <summary>
        /// Config for proxy tests.
        /// </summary>
        public ClassProxyRuntimeTestConfig ClassProxyConfig { get; set; }

        /// <summary>
        /// Type of test definition, proxy or normal.
        /// </summary>
        internal TestDefinitionType Type { get; set; }

        /// <summary>
        /// For proxy class tests.
        /// </summary>
        internal List<string> LoadErrors { get; set; }

        internal enum TestDefinitionType
        {
            Normal = 0,
            ProxyClassMethod
        }

        internal TestDefinition(MethodInfo method, TestClassDefinition parentClass)
        {
            Method = method;
            ParentClass = parentClass;
            RolesWithAccess = parentClass.DefaultRolesWithAccess;

            Name = Method.Name.SpacifySentence();
            Categories = ParentClass.DefaultCategories;
            Type = TestDefinitionType.Normal;

            SetId();
            InitTestParameters(method);
        }

        /// <summary>
        /// Create a new <see cref="TestDefinition"/>.
        /// </summary>
        public TestDefinition(MethodInfo method, RuntimeTestAttribute testAttribute, TestClassDefinition parentClass)
            : this(method, parentClass)
        {
            Name = testAttribute.Name ?? Method.Name.SpacifySentence();
            Description = testAttribute.Description.EnsureDotAtEndIfNotNull();
            AllowParallelExecution = testAttribute.AllowParallelExecution == null ? default(bool?) : (bool)testAttribute.AllowParallelExecution;
            AllowManualExecution = (testAttribute.AllowManualExecution is bool allowManualExecution ? allowManualExecution : parentClass.DefaultAllowManualExecution);
            RolesWithAccess =  testAttribute.RolesWithAccess ?? parentClass.DefaultRolesWithAccess;
            RunButtonText = testAttribute.RunButtonText;
            RunningButtonText = testAttribute.RunningButtonText;

            var methodParams = Method.GetParameters();
            IsCancellable = (methodParams.FirstOrDefault()?.ParameterType == typeof(CancellationToken));

            Categories = (testAttribute.Categories ?? new string[0])
                .Union((testAttribute.Category == null ? new string[0] : new []{ testAttribute.Category }))
                .Distinct()
                .ToList();

            if (!Categories.Any())
            {
                Categories = ParentClass.DefaultCategories;
            }
        }

        /// <summary>
        /// Create a new <see cref="TestDefinition"/>.
        /// </summary>
        public TestDefinition(MethodInfo method, ClassProxyRuntimeTestsAttribute proxyAttribute, ClassProxyRuntimeTestConfig config, TestClassDefinition parentClass)
            : this(method, parentClass)
        {
            RolesWithAccess = proxyAttribute.RolesWithAccess ?? parentClass.DefaultRolesWithAccess;
            Type = TestDefinitionType.ProxyClassMethod;
            ClassProxyConfig = config;
        }

        private void SetId()
        {
            var methodParametersSignature = string.Join("-", Method.GetParameters().Select(x => x.ParameterType.GetFriendlyTypeName()));
            Id = $"{ParentClass.ClassType.FullName}.{Method.Name}.{methodParametersSignature}";
        }

        private void InitTestParameters(MethodInfo method)
        {
            var parameterAttributesOnMethod = method.GetCustomAttributes<RuntimeTestParameterAttribute>(true);

            var methodParameters = Method.GetParameters();
            if (methodParameters.FirstOrDefault()?.ParameterType == typeof(CancellationToken))
            {
                methodParameters = methodParameters.Skip(1).ToArray();
            }

            Parameters = new TestParameter[methodParameters.Length];
            for(int i = 0; i < methodParameters.Length; i++)
            {
                var parameter = methodParameters[i];
                var parameterAttributesOnParameter = parameter.GetCustomAttribute<RuntimeTestParameterAttribute>(true);
                var parameterAttribute = parameterAttributesOnParameter ?? parameterAttributesOnMethod.FirstOrDefault(x => x.Target == parameter.Name);

                Parameters[i] = new TestParameter()
                {
                    Index = i,
                    Name = parameterAttribute?.Name ?? parameter.Name.SpacifySentence(),
                    Description = parameterAttribute?.Description.EnsureDotAtEndIfNotNull(),
                    DefaultValue = GetDefaultValue(parameter, parameterAttribute),
                    ParameterType = parameter.ParameterType,
                    NotNull = parameterAttribute?.UIHints.HasFlag(UIHint.NotNull) == true,
                    ReadOnlyList = parameterAttribute?.UIHints.HasFlag(UIHint.ReadOnlyList) == true,
                    ShowTextArea = parameterAttribute?.UIHints.HasFlag(UIHint.TextArea) == true,
                    FullWidth = parameterAttribute?.UIHints.HasFlag(UIHint.FullWidth) == true,
                    PossibleValues = GetPossibleValues(parameter.ParameterType)
                };
            }
        }

        private List<object> GetPossibleValues(Type parameterType)
        {
            // Enums
            if (parameterType.IsEnum)
            {
                var isFlags = EnumUtils.IsTypeEnumFlag(parameterType);
                var list = new List<object>();
                foreach (var value in Enum.GetValues(parameterType))
                {
                    if (isFlags && (int)value == 0) continue;
                    list.Add(value);
                }
                return list;
            }
            // List of enums
            else if(parameterType.IsGenericType
                && parameterType.GetGenericTypeDefinition() == typeof(List<>)
                && parameterType.GetGenericArguments()[0].IsEnum)
            {
                return GetPossibleValues(parameterType.GetGenericArguments()[0]);
            }
            // Not supported
            else
            {
                return new List<object>();
            }
        }

        private object GetDefaultValue(ParameterInfo parameter, RuntimeTestParameterAttribute parameterAttribute)
        {
            if (parameterAttribute?.DefaultValueFactoryMethod != null)
            {
                var factoryMethod = GetCustomDefaultValueMethod(parameterAttribute, parameter);
                return factoryMethod?.Invoke(null, new object[0]);
            }
            else if (parameter.DefaultValue == null || (parameter.DefaultValue is DBNull))
            {
                return null;
            } else
            {
                return parameter.DefaultValue;
            }
        }

        private MethodInfo GetCustomDefaultValueMethod(RuntimeTestParameterAttribute parameterAttribute, ParameterInfo parameterInfo)
        {
            var classType = Method.DeclaringType;
            return classType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(x => x.Name == parameterAttribute.DefaultValueFactoryMethod 
                    && x.GetParameters().Length == 0
                    && x.ReturnType == parameterInfo.ParameterType);
        }

        /// <summary>
        /// Run the test.
        /// </summary>
        public async Task<TestResult> ExecuteTest(object instance, object[] parameters, bool allowDefaultValues = true,
            Action<CancellationTokenSource> onCancellationTokenCreated = null, bool allowAnyResultType = false)
        {
            var methodParams = Method.GetParameters();
            if (methodParams.FirstOrDefault()?.ParameterType == typeof(CancellationToken))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var paramList = parameters.ToList();
                paramList.Insert(0, cancellationTokenSource.Token);
                parameters = paramList.ToArray();
                onCancellationTokenCreated(cancellationTokenSource);
            }

            var parameterCount = methodParams.Length;
            var parameterList = new object[parameterCount];
            for (int i = 0; i < parameterCount; i++)
            {
                var value = (parameters != null && parameters.Length >= i - 1)
                    ? parameters[i]
                    : null;

                if (allowDefaultValues && value == null && methodParams[i].DefaultValue != null)
                {
                    value = methodParams[i].DefaultValue;
                }

                if (Convert.IsDBNull(value))
                {
                    value = null;
                }

                parameterList[i] = value;
            }

            var returnType = Method.ReturnType;
            if (returnType == typeof(TestResult))
            {
                return (TestResult)Method.Invoke(instance, parameterList);
            }
            else if (returnType == typeof(Task<TestResult>))
            {
                var resultTask = (Task<TestResult>)Method.Invoke(instance, parameterList);
                return await resultTask;
            }
            // Async any
            else if (allowAnyResultType && returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var data = await InvokeAsync(Method, instance, parameterList);
                var result = TestResult.CreateSuccess($"Method {Method} was successfully invoked.")
                    .AddSerializedData(data, TestRunnerService.Serializer);
                return result;
            }
            // Sync any
            else if (allowAnyResultType)
            {
                var data = Method.Invoke(instance, parameterList);
                var result = TestResult.CreateSuccess($"Method {Method} was successfully invoked.")
                    .AddSerializedData(data, TestRunnerService.Serializer);
                return result;
            }
            else
            {
                throw new InvalidTestDefinitionException($"Method {ParentClass.ClassType.Name}.{Method.Name} does not return a TestResult or Task<TestResult>.");
            }
        }

        private async Task<object> InvokeAsync(MethodInfo method, object obj, params object[] parameters)
        {
            var task = (Task)method.Invoke(obj, parameters);
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }

        /// <summary>
        /// Validates the test method.
        /// </summary>
        public TestDefinitionValidationResult Validate()
        {
            var result = new TestDefinitionValidationResult(this);
            var errors = LoadErrors ?? new List<string>();

            if (Type == TestDefinitionType.Normal)
            {
                ValidateNormalTest(errors);
            }

            if (errors.Any())
            {
                result.Error = string.Join("\n", errors);
            }

            return result;
        }

        private void ValidateNormalTest(List<string> errors)
        {
            if (Method.ReturnType != typeof(TestResult) && Method.ReturnType != typeof(Task<TestResult>))
            {
                errors.Add($"Test method '{ParentClass.ClassType.Name}.{Method.Name}' must return a {nameof(TestResult)} or Task<{nameof(TestResult)}>.");
            }

            var methodParameterNames = Method.GetParameters().Select(x => x.Name).ToArray();
            var parameterAttributesOnMethod = Method.GetCustomAttributes<RuntimeTestParameterAttribute>(true);
            var invalidAttributes = parameterAttributesOnMethod.Where(x => x.Target == null || !methodParameterNames.Contains(x.Target));
            if (invalidAttributes.Any())
            {
                var detailItems = new List<string>();
                var nonNullCount = invalidAttributes.Count(x => x.Target != null);
                if (nonNullCount > 0)
                {
                    var targets = invalidAttributes.Where(x => x.Target != null).Select(x => x.Target.QuotifyOrReturnNullText()).JoinForSentence();
                    detailItems.Add($"{nonNullCount} [RuntimeTestParameter]-{"attribute".Pluralize(nonNullCount)} with wrong target value: {targets}");
                }

                var nullCount = invalidAttributes.Count(x => x.Target == null);
                if (nullCount > 0)
                {
                    detailItems.Add($"{nullCount} [RuntimeTestParameter]-{"attribute".Pluralize(nullCount)} with null-value for target");
                }

                var details = $" {detailItems.JoinForSentence()}";
                errors.Add($"Test method '{ParentClass.ClassType.Name}.{Method.Name}' has {details}. " +
                    $"When decorating a method with this attribute it must have a Target value equal to the name of one of its parameters.");
            }

            var parameterAttributes = Method.GetCustomAttributes<RuntimeTestParameterAttribute>(true)
                .Select(x => Tuple.Create(x, Method.GetParameters().FirstOrDefault(p => p.Name == x.Target)))
                .Union(
                    Method.GetParameters()
                    .Select(p => Tuple.Create(p.GetCustomAttribute<RuntimeTestParameterAttribute>(), p))
                    .Where(x => x.Item1 != null)
                )
                .Where(x => x.Item1 != null && x.Item2 != null);
            var attributesWithCustomDefaultValueFactories = parameterAttributes.Where(x => x.Item1.DefaultValueFactoryMethod != null);
            var invalidFactoryReferences = attributesWithCustomDefaultValueFactories.Where(x => GetCustomDefaultValueMethod(x.Item1, x.Item2) == null);
            if (invalidFactoryReferences.Any())
            {
                var factoryRefs = invalidFactoryReferences.Select(x => $"'{x.Item1.DefaultValueFactoryMethod}'").JoinForSentence();
                errors.Add($"Test method '{ParentClass.ClassType.Name}.{Method.Name}' references DefaultValueFactoryMethod(s) that could not be found ({factoryRefs}), make sure it's in the same class, returns the same type as the parameter and is public static.");
            }
        }

        /// <summary>
        /// Test id.
        /// </summary>
        public override string ToString() => Id;
    }
}
