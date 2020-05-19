using HealthCheck.Core.Abstractions;
using Newtonsoft.Json;
using System;

namespace HealthCheck.WebUI.Serializers
{
    /// <summary>
    /// Serializes data using Newtonsoft, ignoring any errors.
    /// </summary>
    public class NewtonsoftJsonSerializer : IJsonSerializer
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

        /// <summary>
        /// Deserialize the given json into an object of the given type.
        /// </summary>
        public T Deserialize<T>(string json)
        {
            try
            {
                return (T)Deserialize(json, typeof(T));
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Deserialize the given json into an object of the given type.
        /// </summary>
        public object Deserialize(string json, Type type)
        {
            try
            {
                return JsonConvert.DeserializeObject(json, type);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
