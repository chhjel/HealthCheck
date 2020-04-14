﻿using System;
using System.Collections.Generic;

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
        /// Values when a selection is possible.
        /// </summary>
        public List<object> PossibleValues { get; set; }

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
    }
}
