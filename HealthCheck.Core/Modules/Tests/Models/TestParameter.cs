using HealthCheck.Core.Models;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// A test method parameter.
    /// </summary>
    public class TestParameter
    {
        /// <summary>
        /// Index of the parameter.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Name of the parameter from code.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the parameter or custom name if provided.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description if provided.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of the parameter.
        /// </summary>
        public Type ParameterType { get; set; }

        /// <summary>
        /// Default parameter value.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Values when a selection is possible.
        /// </summary>
        public List<object> PossibleValues { get; set; }

        /// <summary>
        /// True if the parameter is an out-parameter.
        /// </summary>
        public bool IsOut { get; set; }

        /// <summary>
        /// True if the parameter is a ref-parameter.
        /// </summary>
        public bool IsRef { get; set; }

        /// <summary>
        /// Use to override the label/placeholder/name displayed for any null-value.
        /// </summary>)
        public string NullName { get; set; }

        /// <summary>
        /// True if this is a custom reference parameter.
        /// </summary>
        public bool IsCustomReferenceType { get; set; }

        /// <summary>
        /// Factories for reference data.
        /// </summary>
        public RuntimeTestReferenceParameterFactory ReferenceFactory { get; set; }

        /// <summary>
        /// Any UIHint flags configured for this parameter.
        /// </summary>
        public List<HCUIHint> UIHints { get; set; }

        /// <summary>
        /// Get a matching parameter factory.
        /// </summary>
        public RuntimeTestReferenceParameterFactory GetParameterFactory(TestDefinition test)
        {
            if (IsCustomReferenceType)
            {
                var type = ParameterType;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    type = type.GetGenericArguments()[0];
                }

                if (test?.ClassProxyConfig?.GetFactoryForType(type) != null)
                {
                    return test.ClassProxyConfig.GetFactoryForType(type);
                }
                else if (ReferenceFactory?.CanFactorizeFor(type) != null)
                {
                    return ReferenceFactory;
                }
            }
            return null;
        }
    }
}
