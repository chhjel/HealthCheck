using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Models;
using System;

namespace QoDL.Toolkit.Core.Serializers;

/// <summary>
/// Returns empty strings and nulls.
/// </summary>
public class TKDumpNullJsonSerializer : IJsonSerializer
{
    /// <inheritdoc />
    public string LastError { get; set; }

    /// <summary>
    /// Returns null.
    /// </summary>
    public object Deserialize(string json, Type type) => null;

    /// <summary>
    /// Returns default.
    /// </summary>
    public T Deserialize<T>(string json) => default;

    /// <summary>
    /// Returns success w/ default.
    /// </summary>
    public TKGenericResult<object> DeserializeExt(string json, Type type) => TKGenericResult<object>.CreateSuccess(default);

    /// <summary>
    /// Returns an empty string.
    /// </summary>
    public string Serialize(object obj, bool pretty = true) => "";
}
