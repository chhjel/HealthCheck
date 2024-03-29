using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Services;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Tests.Models;

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
    public TestParameter[] Parameters { get; private set; } = new TestParameter[0];

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
    /// If true, any input will not be included in any audit logging.
    /// </summary>
    public bool HideInputFromAuditLog { get; set; }

    /// <summary>
    /// If true, any result message will not be included in any audit logging.
    /// </summary>
    public bool HideResultMessageFromAuditLog { get; set; }

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
    public ProxyRuntimeTestConfig ClassProxyConfig { get; set; }

    /// <summary>
    /// Type of test definition, proxy or normal.
    /// </summary>
    internal TestDefinitionType Type { get; set; }

    /// <summary>
    /// For proxy class tests.
    /// </summary>
    internal List<string> LoadErrors { get; set; }

    private readonly List<RuntimeTestReferenceParameterFactory> _referenceParameterFactories;

    internal enum TestDefinitionType
    {
        Normal = 0,
        ProxyClassMethod
    }

    internal TestDefinition(MethodInfo method, TestClassDefinition parentClass, List<RuntimeTestReferenceParameterFactory> referenceParameterFactories)
    {
        _referenceParameterFactories = referenceParameterFactories;

        Method = method;
        ParentClass = parentClass;
        RolesWithAccess = parentClass.DefaultRolesWithAccess;

        Name = Method.Name.SpacifySentence();
        Categories = ParentClass.DefaultCategories;
        Type = TestDefinitionType.Normal;

        SetId();
    }

    /// <summary>
    /// Create a new <see cref="TestDefinition"/>.
    /// </summary>
    public TestDefinition(MethodInfo method, RuntimeTestAttribute testAttribute, TestClassDefinition parentClass,
        List<RuntimeTestReferenceParameterFactory> referenceParameterFactories)
        : this(method, parentClass, referenceParameterFactories)
    {
        Name = testAttribute.Name ?? Method.Name.SpacifySentence();
        Description = testAttribute.Description.EnsureDotAtEndIfNotNullOrEmpty();
        AllowParallelExecution = testAttribute.AllowParallelExecution == null ? default(bool?) : (bool)testAttribute.AllowParallelExecution;
        AllowManualExecution = (testAttribute.AllowManualExecution is bool allowManualExecution ? allowManualExecution : parentClass.DefaultAllowManualExecution);
        RolesWithAccess = testAttribute.RolesWithAccess ?? parentClass.DefaultRolesWithAccess;
        RunButtonText = testAttribute.RunButtonText;
        RunningButtonText = testAttribute.RunningButtonText;
        HideInputFromAuditLog = testAttribute.HideInputFromAuditLog;
        HideResultMessageFromAuditLog = testAttribute.HideResultMessageFromAuditLog;

        var methodParams = Method.GetParameters();
        IsCancellable = (methodParams.FirstOrDefault()?.ParameterType == typeof(CancellationToken));

        Categories = (testAttribute.Categories ?? new string[0])
            .Union((testAttribute.Category == null ? new string[0] : new[] { testAttribute.Category }))
            .Distinct()
            .ToList();

        if (!Categories.Any())
        {
            Categories = ParentClass.DefaultCategories;
        }

        InitTestParameters(method, testAttribute.ReferenceParameterFactoryProviderMethodName);
    }

    /// <summary>
    /// Create a new <see cref="TestDefinition"/>.
    /// </summary>
    public TestDefinition(MethodInfo method, ProxyRuntimeTestsAttribute proxyAttribute, ProxyRuntimeTestConfig config, TestClassDefinition parentClass, List<RuntimeTestReferenceParameterFactory> referenceParameterFactories)
        : this(method, parentClass, referenceParameterFactories)
    {
        RolesWithAccess = proxyAttribute.RolesWithAccess ?? parentClass.DefaultRolesWithAccess;
        Type = TestDefinitionType.ProxyClassMethod;
        ClassProxyConfig = config;

        InitTestParameters(method);
    }

    private void SetId()
    {
        var methodParametersSignature = string.Join("-", Method.GetParameters().Select(x => x.ParameterType.GetFriendlyTypeName()));
        Id = $"{ParentClass.ClassType.FullName}.{Method.Name}.{methodParametersSignature}";
    }

    private void InitTestParameters(MethodInfo method, string referenceChoicesFactoryMethodName = null)
    {
        var parameterAttributesOnMethod = method.GetCustomAttributes<RuntimeTestParameterAttribute>(true);

        var methodParameters = Method.GetParameters();
        if (methodParameters.FirstOrDefault()?.ParameterType == typeof(CancellationToken))
        {
            methodParameters = methodParameters.Skip(1).ToArray();
        }

        Parameters = new TestParameter[methodParameters.Length];
        for (int i = 0; i < methodParameters.Length; i++)
        {
            var parameter = methodParameters[i];
            var type = parameter.ParameterType;
            var isOut = parameter.ParameterType.IsByRef && parameter.IsOut;
            var isRef = parameter.ParameterType.IsByRef && !parameter.IsOut;
            if (isRef || isOut)
            {
                type = type.GetElementType();
            }

            var parameterAttributesOnParameter = parameter.GetCustomAttribute<RuntimeTestParameterAttribute>(true);
            var parameterAttribute = parameterAttributesOnParameter ?? parameterAttributesOnMethod.FirstOrDefault(x => x.Target == parameter.Name);

            var referenceFactory = TryFindParameterFactory(referenceChoicesFactoryMethodName, parameter);
            var isCustomReferenceType = referenceFactory != null;

            var uiHints = TKEnumUtils.GetFlaggedEnumValues<TKUIHint>(parameterAttribute?.UIHints) ?? new();
            // Ensure FullWidth if CodeArea
            if (uiHints.Contains(TKUIHint.CodeArea) && !uiHints.Contains(TKUIHint.FullWidth)) uiHints.Add(TKUIHint.FullWidth);
            // Ensure NotNull if CodeArea
            if (uiHints.Contains(TKUIHint.CodeArea) && !uiHints.Contains(TKUIHint.NotNull)) uiHints.Add(TKUIHint.NotNull);

            var hasDefaultValueFactory = parameterAttribute?.DefaultValueFactoryMethod != null;
            var defaultValue = hasDefaultValueFactory ? null : GetDefaultValue(parameter, parameterAttribute);
            Func<object> defaultValueFactory = hasDefaultValueFactory ? () => GetDefaultValue(parameter, parameterAttribute) : null;
            Parameters[i] = new TestParameter()
            {
                Index = i,
                Id = parameter.Name,
                Name = parameterAttribute?.Name ?? parameter.Name.SpacifySentence(),
                Description = parameterAttribute?.Description.EnsureDotAtEndIfNotNullOrEmpty(),
                DefaultValue = defaultValue,
                DefaultValueFactory = defaultValueFactory,
                ParameterType = type,
                UIHints = uiHints,
                NullName = parameterAttribute?.NullName,
                TextPattern = parameterAttribute?.TextPattern,
                CodeLanguage = parameterAttribute?.CodeLanguage ?? "json",
                PossibleValues = TKCustomPropertyAttribute.GetPossibleValues(parameter.ParameterType),
                IsCustomReferenceType = isCustomReferenceType,
                ReferenceFactory = referenceFactory,
                IsOut = isOut,
                IsRef = isRef
            };
        }
    }

    private RuntimeTestReferenceParameterFactory TryFindParameterFactory(
        string referenceChoicesFactoryMethodName, ParameterInfo parameter)
    {
        var type = parameter.ParameterType;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            type = type.GetGenericArguments()[0];
        }

        // First check proxy config
        var factoryFromProxyConfig = ClassProxyConfig?.GetFactoryForType(type);
        if (factoryFromProxyConfig != null)
        {
            return factoryFromProxyConfig;
        }

        // Then check for factory method referenced by name from attribute
        if (referenceChoicesFactoryMethodName != null)
        {
            var factoryProviderMethod = ParentClass.ClassType
                                    .GetMethod(referenceChoicesFactoryMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (factoryProviderMethod != null
                && factoryProviderMethod.GetParameters().Length == 0
                && factoryProviderMethod.ReturnType == typeof(List<RuntimeTestReferenceParameterFactory>))
            {
                try
                {
                    var factories = factoryProviderMethod.Invoke(null, new object[0]) as List<RuntimeTestReferenceParameterFactory>;
                    var factory = factories.FirstOrDefault(x => x.CanFactorizeFor(type));
                    if (factory != null)
                    {
                        return factory;
                    }
                }
                catch (Exception) { /* silence... */ }
            }
        }

        // Finally check global testmodule config factories
        return _referenceParameterFactories?.FirstOrDefault(x => x.CanFactorizeFor(type));
    }

    private static readonly TKSimpleMemoryCache _defaultValueCache = new();
    private object GetDefaultValue(ParameterInfo parameter, RuntimeTestParameterAttribute parameterAttribute)
    {
        if (parameterAttribute?.DefaultValueFactoryMethod != null)
        {
            var factoryMethod = GetCustomDefaultValueMethod(parameterAttribute, parameter);
            var factoryParameters = factoryMethod?.GetParameters()?.Length == 0
                ? new object[0]
                : new object[] { parameter.Name };
            var value = factoryMethod?.Invoke(null, factoryParameters);
            if (value is ITKDefaultTestParameterValue valueWithCache)
            {
                var cacheKey = $"{Id}|{parameter.Position}";
                if (valueWithCache.CacheDuration != null && _defaultValueCache.TryGetValue<object>(cacheKey, out var cachedValue))
                {
                    return cachedValue;
                }
                value = valueWithCache.DefaultValueAsObj;
                if (valueWithCache.CacheDuration != null)
                {
                    _defaultValueCache.Set<object>(cacheKey, value, valueWithCache.CacheDuration.Value);
                }
            }
            return value;
        }
        else if (parameter.DefaultValue == null || (parameter.DefaultValue is DBNull))
        {
            return null;
        }
        // Nullable<Enum> types have integers as default value..
        else if (parameter.ParameterType.IsNullable() && parameter.ParameterType.GenericTypeArguments[0].IsEnum)
        {
            return Enum.ToObject(parameter.ParameterType.GenericTypeArguments[0], parameter.DefaultValue);
        }
        else
        {
            return parameter.DefaultValue;
        }
    }

    private MethodInfo GetCustomDefaultValueMethod(RuntimeTestParameterAttribute parameterAttribute, ParameterInfo parameterInfo)
    {
        var classType = Method.DeclaringType;
        return classType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(x => x.Name == parameterAttribute.DefaultValueFactoryMethod
                && (x.GetParameters().Length == 0 || (x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(string)))
                && (x.ReturnType == parameterInfo.ParameterType
                    || IsDefaultTestParameterWrapperWithType(x.ReturnType, parameterInfo.ParameterType))
            );
    }

    private static bool IsDefaultTestParameterWrapperWithType(Type wrapper, Type inner)
        => wrapper.IsGenericType
            && wrapper.GetGenericTypeDefinition() == typeof(TKDefaultTestParameterValue<>)
            && wrapper.GetGenericArguments()[0] == inner;

    /// <summary>
    /// Run the test.
    /// </summary>
    public async Task<TestResult> ExecuteTest(object instance, object[] parameters, bool allowDefaultValues = true,
        Action<CancellationTokenSource> onCancellationTokenCreated = null,
        bool allowAnyResultType = false, bool includeAutoCreateResult = false)
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
        var refsIndices = new List<int>();
        for (int i = 0; i < parameterCount; i++)
        {
            var methodParam = methodParams[i];
            var value = (parameters != null && parameters.Length >= i - 1)
                ? parameters[i]
                : null;

            if (allowDefaultValues && value == null && methodParam.DefaultValue != null)
            {
                value = methodParam.DefaultValue;
            }

            if (Convert.IsDBNull(value))
            {
                value = null;
            }

            parameterList[i] = value;

            var isOut = methodParam.ParameterType.IsByRef && methodParam.IsOut;
            var isRef = methodParam.ParameterType.IsByRef && !methodParam.IsOut;
            if (isRef || isOut)
            {
                refsIndices.Add(i);
            }
        }

        TestResult postProcessResult(TestResult result)
        {
            if (TestRunnerService.Serializer != null)
            {
                foreach (var index in refsIndices)
                {
                    var methodParam = methodParams[index];
                    var value = parameterList[index];

                    var isOut = methodParam.ParameterType.IsByRef && methodParam.IsOut;
                    var prefix = isOut ? "out" : "ref";

                    if (methodParam.ParameterType.GetElementType().IsPrimitive)
                    {
                        result.AddHtmlData($"Value = <code>{value}</code>", $"{prefix} {methodParam.Name}")
                            .SetLatestDataCleanMode();
                    }
                    else
                    {
                        result.AddSerializedData(value, TestRunnerService.Serializer, $"{prefix} {methodParam.Name}");
                    }
                }
            }
            return result;
        }

        var returnType = Method.ReturnType;
        if (returnType == typeof(TestResult))
        {
            var result = (TestResult)Method.Invoke(instance, parameterList);
            return postProcessResult(result);
        }
        else if (returnType == typeof(Task<TestResult>))
        {
            var resultTask = (Task<TestResult>)Method.Invoke(instance, parameterList);
            var result = await resultTask;
            return postProcessResult(result);
        }
        // Async any
        else if (allowAnyResultType &&
            ((returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            || returnType == typeof(Task)))
        {
            var isResultless = (returnType == typeof(Task) || returnType == typeof(void));
            var fallbackValue = isResultless ? null : "null";
            var data = await InvokeAsync(Method, instance, parameterList);
            if (returnType == typeof(Task))
            {
                data = null;
            }

            var dataTypeName = data?.GetType().GetFriendlyTypeName();
            var dataResultTitle = (data == null) ? "Result" : $"Result of type '{dataTypeName}'";

            var result = TestResult.CreateSuccess($"Method {Method?.Name} was successfully invoked.")
                .SetProxyTestResultObject(data)
                .AddAutoCreatedResultData(data ?? fallbackValue, includeAutoCreateResult && !isResultless)
                .AddSerializedData(data ?? fallbackValue, TestRunnerService.Serializer, dataResultTitle);
            return postProcessResult(result);
        }
        // Sync any
        else if (allowAnyResultType)
        {
            var isResultless = (returnType == typeof(Task) || returnType == typeof(void));
            var fallbackValue = isResultless ? null : "null";
            var data = Method.Invoke(instance, parameterList);

            var dataTypeName = data?.GetType().GetFriendlyTypeName();
            var dataResultTitle = (data == null) ? "Result" : $"Result of type '{dataTypeName}'";

            var result = TestResult.CreateSuccess($"Method {Method?.Name} was successfully invoked.")
                .SetProxyTestResultObject(data)
                .AddAutoCreatedResultData(data ?? fallbackValue, includeAutoCreateResult && !isResultless)
                .AddSerializedData(data ?? fallbackValue, TestRunnerService.Serializer, dataResultTitle);
            return postProcessResult(result);
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

        if (Type == TestDefinitionType.Normal && !errors.Any())
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
            errors.Add($"Test method '{ParentClass.ClassType.Name}.{Method.Name}' references DefaultValueFactoryMethod(s) that could not be found ({factoryRefs}), make sure it's in the same class, is public static, and returns the same type as the parameter or TKDefaultTestParameterValue<T> where T is the parameter type.");
        }
    }

    /// <summary>
    /// Test id.
    /// </summary>
    public override string ToString() => Id;
}
