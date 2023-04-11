using QoDL.Toolkit.Core.Util.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport.Models
{
    /// <summary>
    /// Config for a value formatter.
    /// </summary>
    public class TKDataExportValueFormatterConfig
    {
        /// <summary>
        /// Id of the formatter to use.
        /// </summary>
        public string FormatterId { get; set; }

        /// <summary>
        /// Target property.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Any custom parameters.
        /// </summary>
        public Dictionary<string, string> CustomParameters { get; set; }

        /// <summary>
        /// Used internally to cache built parameters object.
        /// </summary>
        internal object Parameters { get; set; }
    }
}
