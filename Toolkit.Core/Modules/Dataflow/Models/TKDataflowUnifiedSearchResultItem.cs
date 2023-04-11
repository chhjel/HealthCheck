using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Util;

namespace QoDL.Toolkit.Core.Modules.Dataflow.Models
{
    /// <summary>
    /// Result item from unified search.
    /// </summary>
    public class TKDataflowUnifiedSearchResultItem
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
        /// Optional value to group by. Can be used to group results from multiple streams together.
        /// </summary>
        public string GroupByKey { get; set; }

        /// <summary>
        /// Attempts to create reasonable html from the given objects properties.
        /// <para>Shortcut to <c>PopupBody = TKObjectUtils.TryCreateHtmlSummaryFromProperties(..)</c></para>
		/// <para>To ignore properties apply <see cref="TKExcludeFromHtmlSummaryAttribute"/> to them.</para>
        /// </summary>
        public TKDataflowUnifiedSearchResultItem TryCreatePopupBodyFrom(object obj, bool spacifyPropertyNames = true, bool tryParseUrls = true)
        {
            PopupBody = TKObjectUtils.TryCreateHtmlSummaryFromProperties(obj, spacifyPropertyNames, tryParseUrls);
            return this;
        }
    }
}
