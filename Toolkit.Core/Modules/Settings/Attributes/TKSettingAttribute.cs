using QoDL.Toolkit.Core.Attributes;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Settings.Attributes
{
    /// <summary>
    /// Apply to a property to customize how it will look in the toolkit settings page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TKSettingAttribute : TKCustomPropertyAttribute
    {
        /// <summary>
        /// Settings will be grouped by this value.
        /// </summary>
        public string GroupName { get; set; }

        /// <inheritdoc/>
        protected override Dictionary<string, string> GetExtraValues() => new()
        {
            {  "GroupName", GroupName }
        };
    }
}
