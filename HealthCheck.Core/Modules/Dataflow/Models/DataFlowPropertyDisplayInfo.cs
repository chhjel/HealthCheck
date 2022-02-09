using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow.Models
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
        /// <para>Custom filter logic must be added in <see cref="IDataflowStream{TAccessRole}.GetLatestStreamEntriesAsync(DataflowStreamFilter)"/>
        /// or in FilterEntries(..) if FlatFileStoredDataflowStream is used.</para>
        /// </summary>
        public bool IsFilterable { get; set; }

        /// <summary>
        /// Options for a property on an entry coming from a dataflow source.
        /// </summary>
        public DataFlowPropertyDisplayInfo(string propertyName,
            DataFlowPropertyUIHint? uiHint = null, DataFlowPropertyUIVisibilityOption? visibility = null)
        {
            PropertyName = propertyName;
            if (uiHint != null)
            {
                UIHint = uiHint.Value;
            }
            if (visibility != null)
            {
                Visibility = visibility.Value;
            }
        }

        #region Method chaining
        /// <summary>
        /// Order of this property. Lower = earlier.
        /// <para>Default: 99999999</para>
        /// </summary>
        public DataFlowPropertyDisplayInfo SetUIOrder(int order)
        {
            UIOrder = order;
            return this;
        }

        /// <summary>
        /// Option for the properties visibility.
        /// </summary>
        public DataFlowPropertyDisplayInfo SetVisibility(DataFlowPropertyUIVisibilityOption visibility)
        {
            Visibility = visibility;
            return this;
        }

        /// <summary>
        /// Shortcut for: SetVisibility(DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded)
        /// </summary>
        public DataFlowPropertyDisplayInfo OnlyShowWhenExpanded()
        {
            SetVisibility(DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded);
            return this;
        }

        /// <summary>
        /// Shortcut for: SetVisibility(DataFlowPropertyUIVisibilityOption.OnlyInList)
        /// </summary>
        public DataFlowPropertyDisplayInfo OnlyShowInList()
        {
            SetVisibility(DataFlowPropertyUIVisibilityOption.OnlyInList);
            return this;
        }

        /// <summary>
        /// Hint of how to display this field.
        /// <para>The following hints will default to only show in expanded mode unless <paramref name="attemptToBeClever"/> is set to false:</para>
        /// <para>Dictionary, List, Image, Preformatted</para>
        /// </summary>
        public DataFlowPropertyDisplayInfo SetUIHint(DataFlowPropertyUIHint uiHint, bool attemptToBeClever = true)
        {
            UIHint = uiHint;

            if (attemptToBeClever
                && (uiHint == DataFlowPropertyUIHint.Dictionary
                    || uiHint == DataFlowPropertyUIHint.Preformatted
                    || uiHint == DataFlowPropertyUIHint.List
                    || uiHint == DataFlowPropertyUIHint.Image))
            {
                OnlyShowWhenExpanded();
            }
            return this;
        }

        /// <summary>
        /// Name of the property to display.
        /// </summary>
        public DataFlowPropertyDisplayInfo SetDisplayName(string displayName)
        {
            DisplayName = displayName;
            return this;
        }

        /// <summary>
        /// Show the property as html, hide title and only show in list.
        /// <para>Shortcut to: .SetDisplayName("").SetUIHint(DataFlowPropertyUIHint.HTML).OnlyShowInList()</para>
        /// </summary>
        public DataFlowPropertyDisplayInfo SetHtmlIcon()
        {
            SetDisplayName("").SetUIHint(DataFlowPropertyUIHint.HTML).OnlyShowInList();
            return this;
        }

        /// <summary>
        /// Show the property as icon. Property value should be a constant from <see cref="HCMaterialIcons"/>.
        /// </summary>
        public DataFlowPropertyDisplayInfo SetSVGIcon()
        {
            SetDisplayName("").SetUIHint(DataFlowPropertyUIHint.Icon).OnlyShowInList();
            return this;
        }

        /// <summary>
        /// Sets the display name to a prettified version of the property name.
        /// </summary>
        public DataFlowPropertyDisplayInfo PrettifyDisplayName()
        {
            DisplayName = PropertyName.SpacifySentence();
            return this;
        }

        /// <summary>
        /// Allow the property to be filtered upon.
        /// </summary>
        public DataFlowPropertyDisplayInfo SetFilterable(bool isFilterable = true)
        {
            IsFilterable = isFilterable;
            return this;
        }
        #endregion

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
            /// Display as date, time or both. Can be used for <see cref="DateTime"/> and <see cref="DateTimeOffset"/> property types.
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
            HTML,

            /// <summary>
            /// Display as a <see cref="HCMaterialIcons"/> SVG icon.
            /// <para>Value should be a constant from <see cref="HCMaterialIcons"/>.</para>
            /// </summary>
            Icon
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
        /// Format of the property if it's a <see cref="DateTime"/> or <see cref="DateTimeOffset"/> and <see cref="UIHint"/> is set to <see cref="DataFlowPropertyUIHint.DateTime"/>.
        /// </summary>
        public string DateTimeFormat { get; set; } = "MMMM d, yyyy HH:mm:ss";
    }
}
