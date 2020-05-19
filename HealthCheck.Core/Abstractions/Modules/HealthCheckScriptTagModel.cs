using HealthCheck.Core.Extensions;
using HealthCheck.Core.Util;
using System.Collections.Generic;

namespace HealthCheck.Core.Abstractions.Modules
{
    /// <summary>
    /// Represents a simple script-tag.
    /// </summary>
    public class HealthCheckScriptTagModel
    {
        /// <summary>
        /// src-attribute value.
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// Inline script content.
        /// </summary>
        public string InlineScript { get; set; }

        /// <summary>
        /// Create a script tag with the given src value.
        /// </summary>
        public static HealthCheckScriptTagModel CreateSrc(string url)
            => new HealthCheckScriptTagModel { Src = url };

        /// <summary>
        /// Create a script tag with the given inline script.
        /// </summary>
        public static HealthCheckScriptTagModel CreateInline(string script)
            => new HealthCheckScriptTagModel { InlineScript = script };

        /// <summary>
        /// Returns the built script element string.
        /// </summary>
        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(InlineScript))
            {
                return $"<script>{InlineScript}</script>";
            }

            var attributes = new Dictionary<string, string>()
                {
                    { "src", Src }
                };

            var attributesString = HtmlBuilder.BuildAttributes(attributes)
                .EnsureEndsWithIfNotNullOrEmpty(" ");
            return $"<script {attributesString}/>";
        }
    }
}
