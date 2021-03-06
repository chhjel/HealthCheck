using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Utils;
using System;
using System.Collections.Generic;
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
        /// If enabled, any parameter non-supported parameter type will be attempted supported through json-serialization.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool AllowAnyParameterType { get; set; } = true;

        /// <summary>
        /// Action executed on results when <see cref="TestResult.AddAutoCreatedResultData"/> is used, and also for proxy test results.
        /// </summary>
        public Action<TestResult, object> AutoResultAction { get; set; } = AutoResultHelper.DefaultAutoResultAction;

    }
}
