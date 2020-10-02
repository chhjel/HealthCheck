using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly List<RuntimeTestReferenceParameterFactory> _parameterFactories = new List<RuntimeTestReferenceParameterFactory>();

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

        /// <summary>
        /// Add factory methods for reference parameter types.
        /// </summary>
        /// <typeparam name="TParameter">Type of the parameter.</typeparam>
        /// <param name="choicesFactory">Method that returns all choices that the user can select.</param>
        /// <param name="getInstanceByIdFactory">Method that uses id from <paramref name="choicesFactory"/> and returns the matching instance.</param>
        public ProxyRuntimeTestConfig AddParameterTypeConfig<TParameter>(
            Func<IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactory,
            Func<string, object> getInstanceByIdFactory
        ) => AddParameterTypeConfig(typeof(TParameter), choicesFactory, getInstanceByIdFactory);

        /// <summary>
        /// Add factory methods for reference parameter types.
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
            _parameterFactories.Add(new RuntimeTestReferenceParameterFactory(type, choicesFactory, getInstanceByIdFactory));
            return this;
        }

        /// <summary>
        /// Add factory methods for reference parameter types.
        /// <para>This overload can be used to support all subtypes of a given type.</para>
        /// </summary>
        /// <typeparam name="TParameterBaseType">Base type of the parameter.</typeparam>
        /// <param name="choicesFactoryByType">Method that returns all choices that the user can select.</param>
        /// <param name="getInstanceByIdFactoryByType">Method that uses id from <paramref name="choicesFactoryByType"/> and returns the matching instance.</param>
        public ProxyRuntimeTestConfig AddParameterTypeConfig<TParameterBaseType>(
            Func<Type, IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactoryByType,
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
            Func<Type, IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactoryByType,
            Func<Type, string, object> getInstanceByIdFactoryByType
        )
        {
            _parameterFactories.Add(new RuntimeTestReferenceParameterFactory(baseType, choicesFactoryByType, getInstanceByIdFactoryByType));
            return this;
        }

    }
}
