using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// Config for proxy tests.
    /// </summary>
    public class ProxyRuntimeTestConfig
    {
        /// <summary>
        /// Type of object to invoke members on.
        /// </summary>
        public Type TargetClassType { get; set; }

        /// <summary>
        /// Optional instance factory for <see cref="TargetClassType"/>.
        /// </summary>
        public Func<object> InstanceFactory { get; set; }

        /// <summary>
        /// Config for class proxy tests.
        /// </summary>
        public ProxyRuntimeTestConfig(Type targetClassType)
        {
            TargetClassType = targetClassType;
        }

        internal Dictionary<Type, RuntimeTestReferenceParameterFactory> ParameterFactories { get; } = new Dictionary<Type, RuntimeTestReferenceParameterFactory>();

        /// <summary>
        /// Add factory methods for reference any parameter types.
        /// </summary>
        /// <typeparam name="TParameter">Type of the parameter.</typeparam>
        /// <param name="choicesFactory">Method that returns all choices that the user can select.</param>
        /// <param name="getInstanceByIdFactory">Method that uses id from <paramref name="choicesFactory"/> and returns the matching instance.</param>
        public ProxyRuntimeTestConfig AddParameterTypeConfig<TParameter>(
            Func<IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactory,
            Func<string, object> getInstanceByIdFactory
        ) => AddParameterTypeConfig(typeof(TParameter), choicesFactory, getInstanceByIdFactory);

        /// <summary>
        /// Add factory methods for reference any parameter types.
        /// </summary>
        /// <param name="type">Type of the parameter.</param>
        /// <param name="choicesFactory">Method that returns all choices that the user can select.</param>
        /// <param name="getInstanceByIdFactory">Method that uses id from <paramref name="choicesFactory"/> and returns the matching instance.</param>
        public ProxyRuntimeTestConfig AddParameterTypeConfig(
            Type type,
            Func<IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactory,
            Func<string, object> getInstanceByIdFactory
        )
        {
            ParameterFactories[type] = new RuntimeTestReferenceParameterFactory(type, choicesFactory, getInstanceByIdFactory);
            return this;
        }
    }
}
