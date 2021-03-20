using Newtonsoft.Json;
using System;
using System.IO;

namespace HealthCheck.WebUI.Extensions
{
    /// <summary>
    /// Extensions for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Attempts to pretty-format the given json.
        /// <para>Returns the original string if it fails.</para>
        /// </summary>
        public static string TryPrettifyJson(this string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return json;
                }

                using var stringReader = new StringReader(json);
                using var stringWriter = new StringWriter();
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
            catch (Exception)
            {
                return json;
            }
        }
    }
}
