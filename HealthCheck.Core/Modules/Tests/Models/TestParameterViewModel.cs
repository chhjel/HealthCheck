using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// View model for a <see cref="TestParameter"/>.
    /// </summary>
    public class TestParameterViewModel
    {
        /// <summary>
        /// Index of the parameter.
        /// </summary>
        public int Index { get; set; }

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
        /// Values when a selection is possible.
        /// </summary>
        public List<string> PossibleValues { get; set; }

        /// <summary>
        /// Do not allow null-values to be entered in the user interface. Does not affect nullable parameters.
        /// </summary>
        public bool NotNull { get; set; }

        /// <summary>
        /// Only affects generic lists. Does not allow new entries to be added, or existing entries to be changed.
        /// </summary>
        public bool ReadOnlyList { get; set; }

        /// <summary>
        /// Show as text area if this is a string.
        /// </summary>
        public bool ShowTextArea { get; set; }

        /// <summary>
        /// Make the input field full width in size.
        /// </summary>
        public bool FullWidth { get; set; }

        /// <summary>
        /// True when a custom parameter factory has been defined for this type.
        /// </summary>
        public bool IsCustomReferenceType { get; set; }
    }
}
