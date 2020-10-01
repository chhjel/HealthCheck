using System;
using System.Collections.Generic;

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
        public Type ParameterType { get; set; }

        /// <summary>
        /// Create all choices the user can select.
        /// </summary>
        public Func<IEnumerable<RuntimeTestReferenceParameterChoice>> ChoicesFactory { get; set; }

        /// <summary>
        /// Get a single instance of the selected choice from the given selected id.
        /// </summary>
        public Func<string, object> GetInstanceFromIdFactory { get; set; }

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
            ParameterType = parameterType;
            ChoicesFactory = choicesFactory;
            GetInstanceFromIdFactory = getInstanceByIdFactory;
        }
    }
}
