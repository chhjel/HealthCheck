using Newtonsoft.Json;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace QoDL.Toolkit.WebUI.Services;

/// <summary>
/// Provides load/save capabilities to a flatfile.
/// </summary>
public class TKFlatFileStringDictionaryStorage : ITKStringDictionaryStorage
{
    /// <summary>
    /// Path to the file where data will be stored.
    /// </summary>
    protected string FilePath { get; set; }

    private readonly object _fileLock = new();
    private readonly object _valueCacheLock = new();
    private Dictionary<string, string> _valueCache;

    /// <summary>
    /// Provides load/save capabilities to a flatfile.
    /// </summary>
    public TKFlatFileStringDictionaryStorage(string filePath)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    /// <inheritdoc/>
    public void SaveValues(Dictionary<string, string> values)
    {
        var timer = new TKMetricsTimer($"StringDictionaryStorage({Path.GetFileNameWithoutExtension(FilePath)}).SaveData()");

        lock (_valueCacheLock)
        {
            _valueCache = values ?? new Dictionary<string, string>();
        }

        var json = JsonConvert.SerializeObject(values, Formatting.Indented);
        lock (_fileLock)
        {
            File.WriteAllText(FilePath, json);
        }

        TKMetricsContext.AddGlobalTimingValue(timer);
    }

    /// <inheritdoc/>
    public Dictionary<string, string> GetValues()
    {
        lock (_valueCacheLock)
        {
            if (_valueCache != null)
            {
                return _valueCache;
            }
        }

        var values = GetValuesInternal();

        lock (_valueCacheLock)
        {
            _valueCache = values;
        }

        return values;
    }

    private Dictionary<string, string> GetValuesInternal()
    {
        var timer = new TKMetricsTimer($"StringDictionaryStorage({Path.GetFileNameWithoutExtension(FilePath)}).LoadData()");
        string contents;
        try
        {
            if (File.Exists(FilePath))
            {
                lock (_fileLock)
                {
                    contents = TKIOUtils.ReadFile(FilePath);
                }
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(contents)
                    ?? new Dictionary<string, string>();
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }
        catch (Exception)
        {
            return new Dictionary<string, string>();
        }
        finally
        {
            TKMetricsContext.AddGlobalTimingValue(timer);
        }
    }
}
