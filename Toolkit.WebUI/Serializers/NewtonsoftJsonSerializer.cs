using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace QoDL.Toolkit.WebUI.Serializers;

/// <summary>
/// Serializes data using Newtonsoft, ignoring any errors.
/// </summary>
public class NewtonsoftJsonSerializer : IJsonSerializer
{
    /// <summary>
    /// Serializes dumps using Newtonsoft, ignoring any errors.
    /// </summary>
    public string Serialize(object obj, bool pretty = true)
    {
        if (obj != null && !TKGlobalConfig.AllowSerializingType(obj.GetType()))
        {
            return null;
        }

        var settings = new JsonSerializerSettings()
        {
            Formatting = pretty ? Formatting.Indented : Formatting.None,
            Error = (sender, e) =>
            {
                e.ErrorContext.Handled = true;
            },
            Converters = new[] { new StringEnumConverter() },
            ContractResolver = new TKContractResolver()
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
        => DeserializeExt(json, type)?.Data;

    /// <summary>
    /// Deserialize the given json into an object of the given type.
    /// </summary>
    public TKGenericResult<object> DeserializeExt(string json, Type type)
    {
        try
        {
            if (!TKGlobalConfig.AllowSerializingType(type))
            {
                return null;
            }
            var data = JsonConvert.DeserializeObject(json, type);
            return TKGenericResult<object>.CreateSuccess(data);
        }
        catch (Exception ex)
        {
            return TKGenericResult<object>.CreateError(ex);
        }
    }

    private class TKContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty prop = base.CreateProperty(member, memberSerialization);
            prop.Ignored = !TKGlobalConfig.AllowSerializingType(prop.PropertyType);
            return prop;
        }
    }
}
