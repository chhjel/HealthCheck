using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;

namespace HealthCheck.WebUI.Util
{
    /// <summary>
    /// Shared code for .net framework/core login controllers.
    /// </summary>
    internal class HealthCheckLoginControllerHelper
    {
        /// <summary>
        /// Shared code for .net framework/core login controllers.
        /// </summary>
        public HealthCheckLoginControllerHelper()
        {
        }

        /// <summary>
        /// Serializes the given object into a json string.
        /// </summary>
        public static string SerializeJson(object obj, bool stringEnums = true)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            if (stringEnums)
            {
                settings.Converters.Add(new StringEnumConverter());
            }

            return JsonConvert.SerializeObject(obj, settings);
        }

        public static string Generate2FACode(int length = 6)
            => string.Join("", Guid.NewGuid().ToString().Where(x => char.IsDigit(x)).Take(length));
    }
}
