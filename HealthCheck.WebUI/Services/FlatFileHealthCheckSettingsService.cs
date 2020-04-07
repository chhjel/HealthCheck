using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.Settings;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using HealthCheck.Core.Extensions;
using Newtonsoft.Json;
using System.IO;
using System.Linq.Expressions;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// Provides load/save capabilities to a flatfile for settings.
    /// </summary>
    public class FlatFileHealthCheckSettingsService<TSettings> : IHealthCheckSettingsService
        where TSettings: class, new()
    {
        /// <summary>
        /// Path to the settings file.
        /// </summary>
        protected string FilePath { get; set; }

        private readonly object _fileLock = new object();
        private static readonly Dictionary<string, List<HealthCheckSetting>> _defaultSettingsCache = new Dictionary<string, List<HealthCheckSetting>>();
        private static readonly Dictionary<string, List<HealthCheckSetting>> _settingsCache = new Dictionary<string, List<HealthCheckSetting>>();

        /// <summary>
        /// Provides load/save capabilities to a flatfile for settings.
        /// </summary>
        public FlatFileHealthCheckSettingsService(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Get the value of the setting with the given property name.
        /// <para>Use nameof(..) for type safety.</para>
        /// </summary>
        public T GetValue<T>(string settingId)
        {
            var setting = GetSettingItems().FirstOrDefault(x => x.Id == settingId);
            if (setting != null)
            {
                try
                {
                    return (T)setting.Value;
                } catch(Exception) { }
            }

            return default(T);
        }

        /// <summary>
        /// Get the value of the setting with the given property.
        /// <para>E.g. (setting) => setting.ThingIsEnabled</para>
        /// </summary>
        public TValue GetValue<TSetting, TValue>(Expression<Func<TSetting, TValue>> settingProperty)
        {
            var settingId = (settingProperty.Body as MemberExpression)?.Member?.Name;
            if (settingId == null)
            {
                return default(TValue);
            }

            return GetValue<TValue>(settingId);
        }

        /// <summary>
        /// Get the value of the setting with the given property.
        /// <para>E.g. (setting) => setting.ThingIsEnabled</para>
        /// </summary>
        public TValue GetValue<TValue>(Expression<Func<TSettings, TValue>> settingProperty)
            => GetValue<TSettings, TValue>(settingProperty);

        /// <summary>
        /// Load settings from the file.
        /// </summary>
        public virtual List<HealthCheckSetting> GetSettingItems()
        {
            lock (_settingsCache)
            {
                if (_settingsCache.ContainsKey(FilePath))
                {
                    return _settingsCache[FilePath];
                }
            }

            var settings = GetSettingItemsInternal();

            lock (_settingsCache)
            {
                _settingsCache[FilePath] = settings;
            }

            return settings;
        }

        private List<HealthCheckSetting> GetSettingItemsInternal()
        {
            string contents;
            try
            {
                var settingsFromFile = new List<HealthCheckSettingProxy>();
                if (File.Exists(FilePath))
                {
                    lock (_fileLock)
                    {
                        contents = IOUtils.ReadFile(FilePath);
                    }
                    settingsFromFile = JsonConvert.DeserializeObject<List<HealthCheckSettingProxy>>(contents);
                }

                var defaultSettings = GetDefaultSettings();
                foreach (var defSetting in defaultSettings)
                {
                    var setting = settingsFromFile?.FirstOrDefault(x => x.Id == defSetting.Id);
                    if (setting != null)
                    {
                        defSetting.Value = setting.Value;
                    }
                }
                return defaultSettings;
            }
            catch (Exception)
            {
                return new List<HealthCheckSetting>();
            }
        }

        /// <summary>
        /// Save settings to the file.
        /// </summary>
        public virtual void SaveSettings(IEnumerable<HealthCheckSetting> settings)
        {
            var defaultSettings = GetDefaultSettings();
            foreach (var defSetting in defaultSettings)
            {
                var setting = settings?.FirstOrDefault(x => x.Id == defSetting.Id);
                if (setting != null)
                {
                    defSetting.Value = setting.Value;
                }
            }

            var mapped = defaultSettings.Select(x => new HealthCheckSettingProxy
            {
                Id = x.Id,
                Value = x.Value
            }).ToList();

            var json = JsonConvert.SerializeObject(mapped, Formatting.Indented);
            lock (_fileLock)
            {
                File.WriteAllText(FilePath, json);
            }

            lock (_settingsCache)
            {
                _settingsCache[FilePath] = defaultSettings;
            }
        }

        private List<HealthCheckSetting> GetDefaultSettings()
        {
            lock(_defaultSettingsCache)
            {
                if (_defaultSettingsCache.ContainsKey(FilePath))
                {
                    return _defaultSettingsCache[FilePath];
                }
            }

            var settings = new List<HealthCheckSetting>();

            var instance = new TSettings();
            var type = typeof(TSettings);
            var properties = type.GetProperties();
            foreach (var prop in properties)
            {
                try
                {
                    var defValue = prop.GetValue(instance);
                    var setting = new HealthCheckSetting()
                    {
                        Id = prop.Name,
                        Type = prop.PropertyType.Name,
                        DisplayName = prop.Name.SpacifySentence(),
                        Value = defValue
                    };

                    var attribute = prop.GetCustomAttributes(typeof(HealthCheckSettingAttribute), true)
                        .OfType<HealthCheckSettingAttribute>()
                        .FirstOrDefault();
                    if (attribute != null)
                    {
                        setting.DisplayName = attribute.DisplayName ?? setting.DisplayName;
                        setting.Description = attribute.Description;
                        setting.GroupName = attribute.GroupName;
                    }

                    settings.Add(setting);
                }
                catch (Exception) { }
            }

            lock (_defaultSettingsCache)
            {
                _defaultSettingsCache[FilePath] = settings;
            }

            return settings;
        }

        // todo: return wrapper object with any errors.
        //public void AddValidationFor<TPropertyType>(string propertyName, Func<TPropertyType, bool> validator)
        //{
        //}

        private class HealthCheckSettingProxy
        {
            public string Id { get; set; }
            public object Value { get; set; }
        }
    }
}
