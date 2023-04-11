using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.Tests.Models
{
    /// <summary>
    /// Config for proxy tests.
    /// </summary>
    public class ProxyRuntimeTestConfig
    {
        /// <summary>
        /// Type of object to invoke members on.
        /// </summary>
        internal Type TargetClassType { get; set; }

        /// <summary>
        /// Optional instance factory for <see cref="TargetClassType"/>.
        /// </summary>
        internal Func<object> InstanceFactory { get; set; }

        /// <summary>
        /// Optional instance factory for <see cref="TargetClassType"/> with custom context object.
        /// </summary>
        internal Func<object, object> InstanceFactoryWithContext { get; set; }

        /// <summary>
        /// Custom context object factory.
        /// </summary>
        internal Func<object> ContextFactory { get; set; }

        /// <summary>
        /// Custom action to run on test results after execution.
        /// </summary>
        internal Action<TestResult, object> ResultAction { get; set; }

        /// <summary>
        /// Optionally filtering of methods to include.
        /// </summary>
        internal Func<MethodInfo, bool> MethodFilter { get; set; }

        private readonly List<RuntimeTestReferenceParameterFactory> _parameterFactories = new();

        /// <summary>
        /// Config for class proxy tests.
        /// </summary>
        public ProxyRuntimeTestConfig(Type targetClassType)
        {
            TargetClassType = targetClassType;
        }

        /// <summary>
        /// Get a registered parameter factory for the given type.
        /// </summary>
        public RuntimeTestReferenceParameterFactory GetFactoryForType(Type type)
            => _parameterFactories.FirstOrDefault(x => x.CanFactorizeFor(type));

        #region Fluent config
        /// <summary>
        /// Define a custom instance, context and action to run on the result object.
        /// </summary>
        public ProxyRuntimeTestConfig SetCustomContext<T, TContext>(
            Func<TContext> contextFactory,
            Func<TContext, T> instanceFactory,
            Action<TestResult, TContext> resultAction
        ) where TContext : class
        {
            ContextFactory = () => contextFactory();
            ResultAction = (result, context) => resultAction(result, context as TContext);
            InstanceFactoryWithContext = (context) => instanceFactory(context as TContext);
            return this;
        }

        /// <summary>
        /// Define a custom context and action to run on the result object.
        /// </summary>
        public ProxyRuntimeTestConfig SetCustomContext<TContext>(
            Func<TContext> factory,
            Action<TestResult, TContext> resultAction
        ) where TContext: class
        {
            ContextFactory = () => factory();
            ResultAction = (result, context) => resultAction(result, context as TContext);
            return this;
        }

        /// <summary>
        /// Define a custom action to run on the result object.
        /// </summary>
        public ProxyRuntimeTestConfig SetCustomResultAction(Action<TestResult> resultAction)
        {
            ResultAction = (result, context) => resultAction(result);
            return this;
        }

        /// <summary>
        /// Add a custom instance factory.
        /// </summary>
        public ProxyRuntimeTestConfig SetInstanceFactory<T>(Func<T> factory)
        {
            InstanceFactory = () => factory();
            return this;
        }

        /// <summary>
        /// Add a custom instance factory with a custom context object.
        /// <para>This takes priority over <see cref="SetInstanceFactory{T}(Func{T})"/></para>
        /// <para>Custom context can be set from <c>AddCustomContext</c></para>
        /// </summary>
        public ProxyRuntimeTestConfig SetInstanceFactory<T, TContext>(Func<TContext, T> factory)
            where TContext: class
        {
            InstanceFactoryWithContext = (context) => factory(context as TContext);
            return this;
        }

        /// <summary>
        /// Add factory methods for reference parameter types.
        /// </summary>
        /// <typeparam name="TParameter">Type of the parameter.</typeparam>
        /// <param name="choicesFactory">Method that returns all choices that the user can select. Input is filter text.</param>
        /// <param name="getInstanceByIdFactory">Method that uses id from <paramref name="choicesFactory"/> and returns the matching instance.</param>
        public ProxyRuntimeTestConfig AddParameterTypeConfig<TParameter>(
            Func<string, IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactory,
            Func<string, object> getInstanceByIdFactory
        ) => AddParameterTypeConfig(typeof(TParameter), choicesFactory, getInstanceByIdFactory);

        /// <summary>
        /// Add factory methods for reference parameter types.
        /// </summary>
        /// <param name="type">Type of the parameter.</param>
        /// <param name="choicesFactory">Method that returns all choices that the user can select. Input is filter text.</param>
        /// <param name="getInstanceByIdFactory">Method that uses id from <paramref name="choicesFactory"/> and returns the matching instance.</param>
        public ProxyRuntimeTestConfig AddParameterTypeConfig(
            Type type,
            Func<string, IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactory,
            Func<string, object> getInstanceByIdFactory
        )
        {
            _parameterFactories.Add(new RuntimeTestReferenceParameterFactory(type, choicesFactory, getInstanceByIdFactory));
            return this;
        }

        /// <summary>
        /// Add factory methods for reference parameter types.
        /// <para>This overload can be used to support all subtypes of a given type.</para>
        /// </summary>
        /// <typeparam name="TParameterBaseType">Base type of the parameter.</typeparam>
        /// <param name="choicesFactoryByType">Method that returns all choices that the user can select. Input is type of object and filter text.</param>
        /// <param name="getInstanceByIdFactoryByType">Method that uses id from <paramref name="choicesFactoryByType"/> and returns the matching instance.</param>
        public ProxyRuntimeTestConfig AddParameterTypeConfig<TParameterBaseType>(
            Func<Type, string, IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactoryByType,
            Func<Type, string, object> getInstanceByIdFactoryByType
        ) => AddParameterTypeConfig(typeof(TParameterBaseType), choicesFactoryByType, getInstanceByIdFactoryByType);

        /// <summary>
        /// Add factory methods for reference parameter types.
        /// <para>This overload can be used to support all subtypes of a given type.</para>
        /// </summary>
        /// <param name="baseType">Base type of the parameter.</param>
        /// <param name="choicesFactoryByType">Method that returns all choices that the user can select by type.</param>
        /// <param name="getInstanceByIdFactoryByType">Method that uses id from <paramref name="choicesFactoryByType"/> and returns the matching instance by type.</param>
        public ProxyRuntimeTestConfig AddParameterTypeConfig(
            Type baseType,
            Func<Type, string, IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactoryByType,
            Func<Type, string, object> getInstanceByIdFactoryByType
        )
        {
            _parameterFactories.Add(new RuntimeTestReferenceParameterFactory(baseType, choicesFactoryByType, getInstanceByIdFactoryByType));
            return this;
        }

        /// <summary>
        /// Filter methods to include.
        /// </summary>
        public ProxyRuntimeTestConfig SetMethodFilter(Func<MethodInfo, bool> filter)
        {
            MethodFilter = filter;
            return this;
        }
        #endregion
    }
}
