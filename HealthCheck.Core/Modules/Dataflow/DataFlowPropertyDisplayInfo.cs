using System;
using System.Collections.Generic;

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
        /// <para>Default: 99999999</para>
        /// </summary>
        public int UIOrder { get; set; } = 99999999;

        /// <summary>
        /// Hint of how to display this field.
        /// </summary>
        public DataFlowPropertyUIHint UIHint { get; set; }

        /// <summary>
        /// Option for the properties visibility.
        /// </summary>
        public DataFlowPropertyUIVisibilityOption Visibility { get; set; } = DataFlowPropertyUIVisibilityOption.Always;

        /// <summary>
        /// True if a filter should be shown for this property.
        /// </summary>
        public bool IsFilterable { get; set; }

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
            /// Display the data as it is serialized.
            /// </summary>
            Raw = 0,

            /// <summary>
            /// Display as date, time or both. Can be used for <see cref="DateTime"/> property types.
            /// <para>Defaults to 'MMMM d, yyyy @ HH:mm:ss' and can be customized through <see cref="DateTimeFormat"/>.</para>
            /// </summary>
            DateTime,

            /// <summary>
            /// Display as a key/value list. Can be used for <see cref="Dictionary{T,Y}"/> property types.
            /// </summary>
            Dictionary,

            /// <summary>
            /// Display as a list. Can be used for array/list property types.
            /// </summary>
            List,

            /// <summary>
            /// Display as a link using the value as href.
            /// </summary>
            Link,

            /// <summary>
            /// Display as an image using the value as src.
            /// </summary>
            Image,

            /// <summary>
            /// Display in a pre-tag.
            /// </summary>
            Preformatted,

            /// <summary>
            /// Display as HTML
            /// </summary>
            HTML
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
