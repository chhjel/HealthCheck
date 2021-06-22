using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Utils;
using HealthCheck.Core.Util.Modules;
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
        /// <para>Defaults to action, func, expression and delegate types.</para>
        /// </summary>
        public List<Type> HideInputForTypes { get; set; } = new List<Type>
        {
            typeof(Action), typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>), typeof(Action<,,,,>),
            typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>), typeof(Func<,,,,,>),
            typeof(Expression), typeof(Expression<>),
            typeof(Delegate)
        };

        /// <summary>
        /// Must be set in order for <see cref="TestResult.AddFileDownload"/> to work.
        /// <para>Should handle the input and return a <see cref="HealthCheckFileDownloadResult"/> with the matching file to download, or null if no file should be downloaded.</para>
        /// <para>Only type and id values passed to <see cref="TestResult.AddFileDownload"/> will be allowed to be passed to this handler.</para>
        /// <para>Type-value should not contain underscores as it is used as a delimiter in the url.</para>
        /// </summary>
        public FileDownloadHandlerDelegate FileDownloadHandler { get; set; }

        /// <summary>
        /// Definition of the handler for downloading files from <see cref="TestResult.AddFileDownload"/>.
        /// </summary>
        /// <param name="type">Optionally given. Use to e.g. support different sources for files. Should not contain underscores.</param>
        /// <param name="id">Id of the file to download.</param>
        public delegate HealthCheckFileDownloadResult FileDownloadHandlerDelegate(string type, string id);
    }
}
