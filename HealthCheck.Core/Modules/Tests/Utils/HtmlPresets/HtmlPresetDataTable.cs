using HealthCheck.Core.Config;
using System.Web;

namespace HealthCheck.Core.Modules.Tests.Utils.HtmlPresets
{
    /// <summary>
    /// An data-table with sorting and filtering.
    /// </summary>
    public class HtmlPresetDataTable : IHtmlPreset
    {
        /// <summary>
        /// An data-table with sorting and filtering.
        /// </summary>
        public HtmlPresetDataTable()
        {
        }

        /// <summary>
        /// Create html from the data in this object.
        /// </summary>
        public string ToHtml()
        {
            //if (string.IsNullOrWhiteSpace(Text)) return string.Empty;
            var data = new
            {
                IsIt = true,
                YesIt = "Is! Or \"is\" it?"
            };
            var options = HttpUtility.HtmlAttributeEncode(HCGlobalConfig.Serializer.Serialize(data, pretty: false));
            return $"<div data-submodule=\"DataTableSubmodule\" data-submoduleoptions=\"{options}\"></div>";
        }
    }
}
