using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace HealthCheck.Core.Modules.Tests
{
    /// <summary>
    /// Options for <see cref="HCTestsModule"/>.
    /// </summary>
    public class HCTestsModuleOptions
    {
        /// <summary>
        /// The assemblies that contains the test methods.
        /// </summary>
        public IEnumerable<Assembly> AssembliesContainingTests { get; set; }

        /// <summary>
        /// To support custom reference types in tests, a <see cref="RuntimeTestReferenceParameterFactory"/> must be defined for the given type.
        /// </summary>
        public Func<List<RuntimeTestReferenceParameterFactory>> ReferenceParameterFactories { get; set; }

        /// <summary>
        /// To improve default json values, you can specify an instance factory to be serialized as a template here.
        /// <para>If null is returned no template will be used.</para>
        /// </summary>
        public Func<Type, HCTestsJsonTemplateResult> JsonInputTemplateFactory { get; set; }

        /// <summary>
        /// If enabled, any parameter non-supported parameter type will be attempted supported through json-serialization.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool AllowAnyParameterType { get; set; } = true;

        /// <summary>
        /// Allow proxy tests to be discovered.
        /// </summary>
        public bool IncludeProxyTests { get; set; } = true;

        /// <summary>
        /// Action executed on results when <see cref="TestResult.AddAutoCreatedResultData"/> is used, and also for proxy test results.
        /// </summary>
        public Action<TestResult, object> AutoResultAction { get; set; } = AutoResultHelper.DefaultAutoResultAction;

        /// <summary>
        /// Parameter types that will have hidden input fields, including derived types.
        /// </summary>
        public List<Type> HideInputForTypes { get; set; } = new List<Type>
        {
            typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>), typeof(Action<,,,,>),
            typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>), typeof(Func<,,,,,>),
            typeof(Action), typeof(Expression),
            typeof(Expression<>),
            typeof(Expression), typeof(Delegate),
        };
    }
}
