using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Serializers;
using System;
using System.Web;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Helpers for the dump extension methods.
/// </summary>
public static class TKDumpHelper
{
    /// <summary>
    /// Dump the value of an object to the given list.
    /// </summary>
    /// <param name="obj">Object to dump</param>
    /// <param name="serializer">Json serializer implementation</param>
    /// <param name="title">Title of the dump</param>
    public static DataDump Dump<T>(this T obj, IJsonSerializer serializer, string title = null)
    {
        serializer ??= new TKDumpNullJsonSerializer();
        var data = CreateDumpData(obj, serializer);
        return CreateDump<T>(obj?.GetType(), title, data);
    }

    private static string CreateDumpData<T>(T obj, IJsonSerializer serializer)
    {
        if (obj == null)
        {
            return null;
        }

        return serializer?.Serialize(obj) ?? "";
    }

    private static DataDump CreateDump<T>(Type type, string title, string data)
    {
        return new DataDump()
        {
            Title = title ?? CreateDumpName(type),
            Type = type.GetFriendlyTypeName() ?? typeof(T).Name,
            Data = data
        };
    }

    private static string CreateDumpName(Type type) => type.GetFriendlyTypeName();

    internal static string EncodeForJson(bool value)
        => value ? "true" : "false";

    internal static string EncodeForJson(string value)
        => value != null ? $"\"{HttpUtility.JavaScriptStringEncode(value)}\"" : "null";

    internal static string EncodeForJson(DateTimeOffset? date)
    {
        if (!date.HasValue)
        {
            return "null";
        }

        var ticks = date.Value
            .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .TotalMilliseconds;

        return $"\"{ticks}\"";
    }
}
