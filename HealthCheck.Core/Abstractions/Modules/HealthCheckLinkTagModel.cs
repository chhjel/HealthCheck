using HealthCheck.Core.Extensions;
using HealthCheck.Core.Util;
using System.Collections.Generic;

namespace HealthCheck.Core.Abstractions.Modules
{
    /// <summary>
    /// Represents a simple link-tag.
    /// </summary>
    public class HealthCheckLinkTagModel
    {
        /// <summary>
        /// src-attribute value.
        /// </summary>
        public string Rel { get; set; }

        /// <summary>
        /// type-attribute value.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// href-attribute value.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Create a link tag for css with the given href.
        /// </summary>
        public static HealthCheckLinkTagModel CreateStylesheet(string url)
            => new HealthCheckLinkTagModel { Rel = "stylesheet", Type = "text/css", Href = url };

        /// <summary>
        /// Returns the built link element string.
        /// </summary>
        public override string ToString()
        {
            var attributes = new Dictionary<string, string>()
                {
                    { "rel", Rel },
                    { "type", Type },
                    { "href", Href }
                };

            var attributesString = HtmlBuilder.BuildAttributes(attributes)
                .EnsureEndsWithIfNotNullOrEmpty(" ");
            return $"<link {attributesString}/>";
        }
    }
}
