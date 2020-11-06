using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        public string SerializeJson(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            settings.Converters.Add(new StringEnumConverter());

            return JsonConvert.SerializeObject(obj, settings);
        }
    }
}
