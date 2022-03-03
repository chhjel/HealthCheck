using HealthCheck.Core.Attributes;
using HealthCheck.Core.Util;

namespace HealthCheck.Core.Modules.Dataflow.Models
{
    /// <summary>
    /// Result item from unified search.
    /// </summary>
    public class HCDataflowUnifiedSearchResultItem
    {
        /// <summary>
        /// Title of the result.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Body that supports html.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Optional html dialog contents that will be shown when the result item is clicked.
        /// <para>Can be auto-created using <see cref="TryCreatePopupBodyFrom"/>.</para>
        /// </summary>
        public string PopupBody { get; set; }

        /// <summary>
        /// Attempts to create reasonable html from the given objects properties.
        /// <para>Shortcut to <c>PopupBody = HCObjectUtils.TryCreateHtmlSummaryFromProperties(..)</c></para>
		/// <para>To ignore properties apply <see cref="HCExcludeFromHtmlSummaryAttribute"/> to them.</para>
        /// </summary>
        public HCDataflowUnifiedSearchResultItem TryCreatePopupBodyFrom(object obj, bool spacifyPropertyNames = true, bool tryParseUrls = true)
        {
            PopupBody = HCObjectUtils.TryCreateHtmlSummaryFromProperties(obj, spacifyPropertyNames, tryParseUrls);
            return this;
        }
    }
}
