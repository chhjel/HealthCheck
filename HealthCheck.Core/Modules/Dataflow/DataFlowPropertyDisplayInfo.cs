using System;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Options for a property on an entry coming from a dataflow source.
    /// </summary>
    public class DataFlowPropertyDisplayInfo
    {
        /// <summary>
        /// Name of the property to target.
        /// </summary>
        public string PropertyName { get; set; }
        
        /// <summary>
        /// Name of the property to display.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Order of this property. Lower = earlier.
        /// </summary>
        public int UIOrder { get; set; }

        /// <summary>
        /// Hint of how to display this field.
        /// </summary>
        public DataFlowPropertyUIHint UIHint { get; set; }

        /// <summary>
        /// Option for the properties visibility.
        /// </summary>
        public DataFlowPropertyUIVisibilityOption Visibility { get; set; } = DataFlowPropertyUIVisibilityOption.Always;

        /// <summary>
        /// Options for a property on an entry coming from a dataflow source.
        /// </summary>
        public DataFlowPropertyDisplayInfo(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Hint for how to display this property in the ui.
        /// </summary>
        public enum DataFlowPropertyUIHint
        {
            /// <summary>
            /// Display the data as it is.
            /// </summary>
            Raw = 0,

            /// <summary>
            /// Display as date, time or both.
            /// <para>Defaults to 'MMMM d, yyyy @ HH:mm:ss' and can be customized through <see cref="DateTimeFormat"/>.</para>
            /// </summary>
            DateTime
        }

        /// <summary>
        /// Hint for where to display this property in the ui.
        /// </summary>
        public enum DataFlowPropertyUIVisibilityOption
        {
            /// <summary>
            /// Show both in the list and details.
            /// </summary>
            Always = 0,

            /// <summary>
            /// Show only in the expanded section.
            /// </summary>
            OnlyWhenExpanded,

            /// <summary>
            /// Only show in list, not in the expanded section.
            /// </summary>
            OnlyInList,

            /// <summary>
            /// 
            /// </summary>
            Hidden
        }

        /// <summary>
        /// Format of the property if it's a <see cref="DateTime"/> and <see cref="UIHint"/> is set to <see cref="DataFlowPropertyUIHint.DateTime"/>.
        /// </summary>
        public string DateTimeFormat { get; set; } = "MMMM d, yyyy @ HH:mm:ss";
    }
}
