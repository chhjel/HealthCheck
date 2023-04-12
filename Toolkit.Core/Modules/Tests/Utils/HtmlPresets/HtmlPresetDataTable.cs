using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;

/// <summary>
/// An data-table with sorting and filtering.
/// </summary>
public class HtmlPresetDataTable : IHtmlPreset
{
    private string[] _headers;
    private readonly List<string[]> _rows = new();
    private static readonly TKRollingMemoryCache<Type, PropertyInfo[]> _propertyCache = new() { MaxCount = 20 };
    private static readonly TKStringConverter _stringConverter = new();

    /// <summary>
    /// An data-table with sorting and filtering.
    /// </summary>
    public HtmlPresetDataTable()
    {
    }

    /// <summary>
    /// Add items to include in the table.
    /// <para>Top level, public properties will be included.</para>
    /// </summary>
    public HtmlPresetDataTable AddItems(IList<object> items)
    {
        CreateDefinitionIfMissing(items?.FirstOrDefault()?.GetType());
        foreach (var item in items)
        {
            AddItemInternal(item);
        }
        return this;
    }

    /// <summary>
    /// Add item to include in the table.
    /// <para>Top level, public properties will be included.</para>
    /// </summary>
    public HtmlPresetDataTable AddItem(object item)
    {
        CreateDefinitionIfMissing(item?.GetType());
        AddItemInternal(item);
        return this;
    }

    /// <summary>
    /// Create html from the data in this object.
    /// </summary>
    public string ToHtml()
    {
        var data = new
        {
            Headers = _headers,
            Rows = _rows
        };
        var options = HttpUtility.HtmlAttributeEncode(TKGlobalConfig.Serializer.Serialize(data, pretty: false));
        return $"<div data-submodule=\"DataTableSubmodule\" data-submoduleoptions=\"{options}\"></div>";
    }

    private void AddItemInternal(object item)
    {
        if (item == null) return;

        var type = item.GetType();
        var props = GetProperties(type);
        var values = new string[props.Length];
        for (int i = 0; i < props.Length; i++)
        {
            var prop = props[i];
            var value = prop.GetValue(item);
            values[i] = _stringConverter.ConvertToString(value);
        }
        _rows.Add(values);
    }

    private readonly object _defLock = new();
    private void CreateDefinitionIfMissing(Type type)
    {
        lock (_defLock)
        {
            if (_headers != null || type == null) return;
            var props = GetProperties(type);
            _headers = props.Select(x => x.Name).ToArray();
            // types? GetFriendlyTypeName
        }
    }

    private PropertyInfo[] GetProperties(Type type)
        => _propertyCache.GetOrCreate(type, () => type.GetProperties());
}
