using System;

namespace HealthCheck.Core.Entities
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
        /// Do not allow null-values to be entered in the user interface. Does not affect nullable parameters.
        /// </summary>
        public bool NotNull { get; set; }
    }
}
