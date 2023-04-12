using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Enums;
using QoDL.Toolkit.Core.Modules.AccessTokens.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog.Services;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Abstractions;
using QoDL.Toolkit.Core.Modules.Settings.Abstractions;
using QoDL.Toolkit.Core.Modules.Settings.Services;
using QoDL.Toolkit.Core.Modules.SiteEvents.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QoDL.Toolkit.WebUI.Services;

/// <summary>
/// Provides a lazy way to auto-create flatfile service instances for singleton dependency injection.
/// </summary>
public class TKLazyFlatFileFactory
{
    /// <summary>
    /// Used internally from other assemblies.
    /// </summary>
    public static Dictionary<TKModuleType, Func<Func<string, string>, Dictionary<Type, IEnumerable<object>>>> ExternalModuleInstanceFactories { get; set; }
        = new Dictionary<TKModuleType, Func<Func<string, string>, Dictionary<Type, IEnumerable<object>>>>();

    private readonly string _rootFolder;

    /// <summary>
    /// Provides a lazy way to auto-create flatfile instances.
    /// </summary>
    public TKLazyFlatFileFactory(string rootFolder)
    {
        _rootFolder = rootFolder;
        TKGlobalConfig.EnsureInitialized();
    }

    /// <summary>
    /// Lazy way to auto-create flatfile instances.
    /// </summary>
    public void Factorize(Action<Type, object> instanceAction,
        TKModuleType? includedTypes = null,
        Func<Type, object, bool> condition = null)
    {
        var instances = CreateInstances(includedTypes);
        foreach (var kvp in instances)
        {
            var type = kvp.Key;
            foreach (var instance in kvp.Value)
            {
                if (condition?.Invoke(type, instance) != false)
                {
                    instanceAction(type, instance);
                }
            }
        }
    }

    /// <summary>
    /// Create instances of all given types.
    /// </summary>
    public Dictionary<Type, List<object>> CreateInstances(TKModuleType? includedTypes = null)
    {
        var instances = new Dictionary<Type, List<object>>();

        bool includeType(TKModuleType type)
            => includedTypes == null || includedTypes.Value.HasFlag(type);

        if (includeType(TKModuleType.EventNotifications))
        {
            AddItem(instances, typeof(IEventSinkNotificationConfigStorage), new object[] {
                new FlatFileEventSinkNotificationConfigStorage(CreatePath(@"EventSink_EventConfigs.json")),
            });
            AddItem(instances, typeof(IEventSinkKnownEventDefinitionsStorage), new object[] {
                new FlatFileEventSinkKnownEventDefinitionsStorage(CreatePath(@"EventSink_EventDefinitions.json")),
            });
        }

        if (includeType(TKModuleType.SecureFileDownload))
        {
            AddItem(instances, typeof(ISecureFileDownloadDefinitionStorage), new object[] {
                new FlatFileSecureFileDownloadDefinitionStorage(CreatePath(@"SecureFileDownload_Definitions.json")),
            });
        }

        if (includeType(TKModuleType.Messages))
        {
            AddItem(instances, typeof(ITKMessageStorage), new object[] {
                new TKFlatFileMessageStore(CreatePath(@"Messages"))
            });
        }

        if (includeType(TKModuleType.AccessTokens))
        {
            AddItem(instances, typeof(IAccessManagerTokenStorage), new object[] {
                new FlatFileAccessManagerTokenStorage(CreatePath(@"AccessTokens_Data.json"))
            });
        }

        if (includeType(TKModuleType.AuditLog))
        {
            var blobStorage = new FlatFileAuditBlobStorage(CreatePath(@"AuditEvent_Blobs"), maxEventAge: TimeSpan.FromDays(30));
            AddItem(instances, typeof(IAuditEventStorage), new object[] {
                new FlatFileAuditEventStorage(CreatePath(@"AuditEvent_History.json"),
                    maxEventAge: TimeSpan.FromDays(30), delayFirstCleanup: false, blobStorage: blobStorage)
            });
        }

        if (includeType(TKModuleType.SiteEvents))
        {
            AddItem(instances, typeof(ISiteEventStorage), new object[] {
                new FlatFileSiteEventStorage(CreatePath(@"SiteEvent_Storage.json"),
                    maxEventAge: TimeSpan.FromDays(30), delayFirstCleanup: false)
            });
        }

        if (includeType(TKModuleType.Settings))
        {
            AddItem(instances, typeof(ITKSettingsService), new object[] {
                new TKDefaultSettingsService(new TKFlatFileStringDictionaryStorage(CreatePath(@"Settings_Storage.json")))
            });
        }

        TryAddExternalInstances(TKModuleType.EndpointControl, instances, includeType);
        TryAddExternalInstances(TKModuleType.Code, instances, includeType);
        TryAddExternalInstances(TKModuleType.RequestLog, instances, includeType);

        return instances
            .Where(x => x.Key != null && x.Value?.Any() == true)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private void TryAddExternalInstances(TKModuleType moduleType, Dictionary<Type, List<object>> instances, Func<TKModuleType, bool> includeType)
    {
        if (includeType(moduleType)
            && ExternalModuleInstanceFactories.TryGetValue(moduleType, out var extInstanceFactory))
        {
            var items = extInstanceFactory?.Invoke(CreatePath);
            if (items?.Any() == true)
            {
                foreach (var item in items)
                {
                    AddItem(instances, item.Key, item.Value);
                }
            }
        }
    }

    private static void AddItem(Dictionary<Type, List<object>> list, Type type, IEnumerable<object> items)
    {
        if (items?.Any() != true)
        {
            return;
        }

        if (!list.ContainsKey(type))
        {
            list[type] = new List<object>();
        }

        list[type].AddRange(items);
    }

    private string CreatePath(string relative)
        => Path.Combine(_rootFolder, relative);
}
