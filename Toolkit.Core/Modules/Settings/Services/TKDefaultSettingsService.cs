using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Modules.Settings.Abstractions;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Util.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QoDL.Toolkit.Core.Modules.Settings.Services;

/// <inheritdoc/>
public class TKDefaultSettingsService : ITKSettingsService
{
    private readonly ITKStringDictionaryStorage _storage;
    private static readonly Dictionary<string, object> _objCache = new();
    private static readonly Dictionary<string, Dictionary<string, string>> _valuesCache = new();
    private static readonly Dictionary<string, List<TKBackendInputConfig>> _defCache = new();
    private static readonly Semaphore _semaphore = new(1, 1);

    /// <summary>
    /// Provides load/save capabilities for string dictionaries with caching.
    /// </summary>
    public TKDefaultSettingsService(ITKStringDictionaryStorage storage)
    {
        _storage = storage;
    }

    /// <inheritdoc/>
    public TModel GetSettings<TModel>()
        where TModel : class, new()
    {
        var type = typeof(TModel);
        var key = type.FullName;

        try
        {
            _semaphore.WaitOne();

            if (_objCache.ContainsKey(key))
            {
                return _objCache[key] as TModel;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        var values = GetSettingValues(type);
        try
        {
            _semaphore.WaitOne();

            var instance = TKValueConversionUtils.ConvertInputModel<TModel>(values);
            _objCache[key] = instance;
            return instance;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public Dictionary<string, string> GetSettingValues(Type type)
    {
        try
        {
            _semaphore.WaitOne();

            var key = type.FullName;
            if (!_valuesCache.ContainsKey(key))
            {
                var definitions = EnsureDefinitionCache(type, key);

                // Include any missing default values
                var values = _storage.GetValues();
                foreach (var def in definitions)
                {
                    if (!values.ContainsKey(def.Id))
                    {
                        values[def.Id] = def.DefaultValue;
                    }
                }

                _valuesCache[key] = values;
            }

            return _valuesCache[key];
        }
        finally
        {
            _semaphore.Release();
        }
    }
    /// <inheritdoc/>
    public void SaveSettings<TModel>(TModel model)
        where TModel : class, new()
        => SaveSettings(typeof(TModel), model);

    /// <inheritdoc/>
    public void SaveSettings(Type type, object model)
    {
        try
        {
            _semaphore.WaitOne();

            var key = type.FullName;
            var definitions = EnsureDefinitionCache(type, key);

            var converter = new TKStringConverter();
            var values = definitions.ToDictionary(x => x.Id, x => converter.ConvertToString(x.PropertyInfo.GetValue(model)));
            _storage.SaveValues(values);

            _valuesCache[key] = values;
            _objCache[key] = model;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public void SaveSettings(Type type, Dictionary<string, string> values)
    {
        try
        {
            _semaphore.WaitOne();

            var key = type.FullName;
            _storage.SaveValues(values);

            _valuesCache[key] = values;
            _objCache.Remove(key);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static List<TKBackendInputConfig> EnsureDefinitionCache(Type type, string key)
    {
        if (!_defCache.TryGetValue(key, out var def))
        {
            def = TKCustomPropertyAttribute.CreateInputConfigs(type);
            _defCache[key] = def;
        }
        return def;
    }
}
