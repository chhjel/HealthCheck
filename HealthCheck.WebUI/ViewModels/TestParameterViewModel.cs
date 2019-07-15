using HealthCheck.Core.Entities;

namespace HealthCheck.WebUI.ViewModels
{
    /// <summary>
    /// View model for a <see cref="TestParameter"/>.
    /// </summary>
    public class TestParameterViewModel
    {
        /// <summary>
        /// Name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type name of the parameter.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Stringified default value of the parameter.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Do not allow null-values to be entered in the user interface. Does not affect nullable parameters.
        /// </summary>
        public bool NotNull { get; set; }
    }
}
