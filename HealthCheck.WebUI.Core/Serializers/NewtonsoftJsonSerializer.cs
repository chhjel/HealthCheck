using HealthCheck.Core.Abstractions;
using Newtonsoft.Json;

namespace HealthCheck.WebUI.Core.Serializers
{
    /// <summary>
    /// Serializes dumps using Newtonsoft, ignoring any errors.
    /// </summary>
    public class NewtonsoftJsonSerializer : IDumpJsonSerializer
    {
        /// <summary>
        /// Serializes dumps using Newtonsoft, ignoring any errors.
        /// </summary>
        public string Serialize(object obj)
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                Error = (sender, e) =>
                {
                    sender = null;
                    e.ErrorContext.Handled = true;
                }
            };
            return JsonConvert.SerializeObject(obj, settings);
        }
    }
}
