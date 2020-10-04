using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;

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
                    e.ErrorContext.Handled = true;
                },
                Converters = new[] { new StringEnumConverter() },
                ContractResolver = new HCContractResolver()
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

        private class HCContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty prop = base.CreateProperty(member, memberSerialization);
                if (HCGlobalConfig.TypesIgnoredInSerialization
                    ?.Any(x => prop.PropertyType == x) == true)
                {
                    prop.Ignored = true;
                }
                else if (HCGlobalConfig.TypesWithDescendantsIgnoredInSerialization
                    ?.Any(x => x.IsAssignableFrom(prop.PropertyType)) == true)
                {
                    prop.Ignored = true;
                }
                else if (HCGlobalConfig.NamespacePrefixesIgnoredInSerialization
                    ?.Any(x => prop.PropertyType.Namespace?.ToUpper()?.StartsWith(x?.ToUpper()) == true) == true)
                {
                    prop.Ignored = true;
                }
                return prop;
            }
        }
    }
}
