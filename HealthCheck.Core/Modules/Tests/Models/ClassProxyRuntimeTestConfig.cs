using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// Config for class proxy tests.
    /// </summary>
    public class ClassProxyRuntimeTestConfig
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
        public ClassProxyRuntimeTestConfig(Type targetClassType)
        {
            TargetClassType = targetClassType;
        }

        internal Dictionary<Type, ClassProxyRuntimeTestParameterFactory> ParameterFactories { get; } = new Dictionary<Type, ClassProxyRuntimeTestParameterFactory>();

        /// <summary>
        /// Add factory methods for reference any parameter types.
        /// </summary>
        /// <typeparam name="TParameter">Type of the parameter.</typeparam>
        /// <param name="choicesFactory">Method that returns all choices that the user can select.</param>
        /// <param name="getInstanceByIdFactory">Method that uses id from <paramref name="choicesFactory"/> and returns the matching instance.</param>
        public ClassProxyRuntimeTestConfig AddParameterTypeConfig<TParameter>(
            Func<IEnumerable<ClassProxyRuntimeTestParameterChoice>> choicesFactory,
            Func<string, object> getInstanceByIdFactory
        ) => AddParameterTypeConfig(typeof(TParameter), choicesFactory, getInstanceByIdFactory);

        /// <summary>
        /// Add factory methods for reference any parameter types.
        /// </summary>
        /// <param name="type">Type of the parameter.</param>
        /// <param name="choicesFactory">Method that returns all choices that the user can select.</param>
        /// <param name="getInstanceByIdFactory">Method that uses id from <paramref name="choicesFactory"/> and returns the matching instance.</param>
        public ClassProxyRuntimeTestConfig AddParameterTypeConfig(
            Type type,
            Func<IEnumerable<ClassProxyRuntimeTestParameterChoice>> choicesFactory,
            Func<string, object> getInstanceByIdFactory
        )
        {
            ParameterFactories[type] = new ClassProxyRuntimeTestParameterFactory()
            {
                ChoicesFactory = choicesFactory,
                GetInstanceFromIdFactory = getInstanceByIdFactory
            };
            return this;
        }

        internal class ClassProxyRuntimeTestParameterFactory
        {
            public Type ParameterType { get; set; }
            public Func<IEnumerable<ClassProxyRuntimeTestParameterChoice>> ChoicesFactory { get; set; }
            public Func<string, object> GetInstanceFromIdFactory { get; set; }
        }
    }
}
