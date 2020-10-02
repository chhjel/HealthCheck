using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// Creates choices and selected values for reference type parameters.
    /// </summary>
    public class RuntimeTestReferenceParameterFactory
    {
        /// <summary>
        /// Type of the parameter to factorize for.
        /// </summary>
        private readonly Type _parameterType;

        /// <summary>
        /// Allow subtypes of <see cref="_parameterType"/>.
        /// </summary>
        private readonly bool _allowSubTypes;

        /// <summary>
        /// Create all choices the user can select.
        /// </summary>
        private readonly Func<IEnumerable<RuntimeTestReferenceParameterChoice>> _choicesFactory;

        /// <summary>
        /// Create all choices the user can select for the given type.
        /// <para>This takes priority over <see cref="_choicesFactory"/>.</para>
        /// </summary>
        private readonly Func<Type, IEnumerable<RuntimeTestReferenceParameterChoice>> _choicesFactoryByType;

        /// <summary>
        /// Get a single instance of the selected choice from the given selected id.
        /// </summary>
        private readonly Func<string, object> _getInstanceFromIdFactory;

        /// <summary>
        /// Get a single instance of the selected choice from the given selected id.
        /// <para>This takes priority over <see cref="_getInstanceFromIdFactory"/>.</para>
        /// </summary>
        private readonly Func<Type, string, object> _getInstanceFromIdFactoryByType;

        /// <summary>
        /// <param name="parameterType">Type of the parameter to factorize for.</param>
        /// Creates choices and selected values for reference type parameters.
        /// <param name="choicesFactory">Create all choices the user can select.</param>
        /// <param name="getInstanceByIdFactory">Get a single instance of the selected choice from the given selected id.</param>
        /// </summary>
        public RuntimeTestReferenceParameterFactory(
            Type parameterType,
            Func<IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactory,
            Func<string, object> getInstanceByIdFactory)
        {
            _parameterType = parameterType;
            _choicesFactory = choicesFactory;
            _getInstanceFromIdFactory = getInstanceByIdFactory;
        }

        /// <summary>
        /// <param name="baseParameterType">Type of the parameter to factorize for.</param>
        /// Creates choices and selected values for reference type parameters.
        /// <param name="choicesFactoryByType">Create all choices the user can select for a given type.</param>
        /// <param name="getInstanceByIdFactoryByType">Get a single instance of the selected choice from the given selected id for a given type.</param>
        /// </summary>
        public RuntimeTestReferenceParameterFactory(
            Type baseParameterType,
            Func<Type, IEnumerable<RuntimeTestReferenceParameterChoice>> choicesFactoryByType,
            Func<Type, string, object> getInstanceByIdFactoryByType)
        {
            _allowSubTypes = true;
            _parameterType = baseParameterType;
            _choicesFactoryByType = choicesFactoryByType;
            _getInstanceFromIdFactoryByType = getInstanceByIdFactoryByType;
        }

        /// <summary>
        /// Check if type config allows for invoking factory methods for this type.
        /// </summary>
        public bool CanFactorizeFor(Type type)
        {
            if (_parameterType == type) return true;
            else if (_allowSubTypes && _parameterType.IsAssignableFrom(type)) return true;
            else return false;
        }

        /// <summary>
        /// Get all available choices for the given type.
        /// </summary>
        public IEnumerable<RuntimeTestReferenceParameterChoice> GetChoicesFor(Type type)
        {
            if (!CanFactorizeFor(type)) return Enumerable.Empty<RuntimeTestReferenceParameterChoice>();

            return _choicesFactoryByType?.Invoke(type)
                ?? _choicesFactory?.Invoke()
                ?? Enumerable.Empty<RuntimeTestReferenceParameterChoice>();
        }

        /// <summary>
        /// Get a single instance from a selected choice id.
        /// </summary>
        public object GetInstanceByIdFor(Type type, string id)
        {
            if (!CanFactorizeFor(type)) return null;

            return _getInstanceFromIdFactoryByType?.Invoke(type, id)
                ?? _getInstanceFromIdFactory?.Invoke(id);
        }
    }
}
