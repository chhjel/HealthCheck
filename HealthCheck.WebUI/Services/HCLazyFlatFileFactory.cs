using HealthCheck.Core.Config;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Modules.AccessTokens.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Services;
using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// Provides a lazy way to auto-create flatfile service instances for singleton dependency injection.
    /// </summary>
    public class HCLazyFlatFileFactory
    {
        /// <summary>
        /// Used internally from other assemblies.
        /// </summary>
        public static Dictionary<HCModuleType, Func<Func<string, string>, Dictionary<Type, IEnumerable<object>>>> ExternalModuleInstanceFactories { get; set; }
            = new Dictionary<HCModuleType, Func<Func<string, string>, Dictionary<Type, IEnumerable<object>>>>();

        private readonly string _rootFolder;

        /// <summary>
        /// Provides a lazy way to auto-create flatfile instances.
        /// </summary>
        public HCLazyFlatFileFactory(string rootFolder)
        {
            _rootFolder = rootFolder;
            HCGlobalConfig.EnsureInitialized();
        }

        /// <summary>
        /// Lazy way to auto-create flatfile instances.
        /// </summary>
        public void Factorize(Action<Type, object> instanceAction,
            HCModuleType? includedTypes = null,
            Func<Type, object, bool> condition = null)
        {
            var instances = CreateInstances(includedTypes);
            foreach (var kvp in instances)
            {
                var type = kvp.Key;
                foreach(var instance in kvp.Value)
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
        public Dictionary<Type, List<object>> CreateInstances(HCModuleType? includedTypes = null)
        {
            var instances = new Dictionary<Type, List<object>>();

            bool includeType(HCModuleType type)
                => includedTypes == null || includedTypes.Value.HasFlag(type);

            if (includeType(HCModuleType.EventNotifications))
            {
                AddItem(instances, typeof(IEventSinkNotificationConfigStorage), new object[] {
                    new FlatFileEventSinkNotificationConfigStorage(CreatePath(@"EventSink_EventConfigs.json")),
                });
                AddItem(instances, typeof(IEventSinkKnownEventDefinitionsStorage), new object[] {
                    new FlatFileEventSinkKnownEventDefinitionsStorage(CreatePath(@"EventSink_EventDefinitions.json")),
                });
            }

            if (includeType(HCModuleType.SecureFileDownload))
            {
                AddItem(instances, typeof(ISecureFileDownloadDefinitionStorage), new object[] {
                    new FlatFileSecureFileDownloadDefinitionStorage(CreatePath(@"SecureFileDownload_Definitions.json")),
                });
            }

            if (includeType(HCModuleType.Messages))
            {
                AddItem(instances, typeof(IHCMessageStorage), new object[] {
                    new HCFlatFileMessageStore(CreatePath(@"Messages"))
                });
            }

            if (includeType(HCModuleType.AccessTokens))
            {
                AddItem(instances, typeof(IAccessManagerTokenStorage), new object[] {
                    new FlatFileAccessManagerTokenStorage(CreatePath(@"AccessTokens_Data.json"))
                });
            }

            if (includeType(HCModuleType.AuditLog))
            {
                var blobStorage = new FlatFileAuditBlobStorage(CreatePath(@"AuditEvent_Blobs"), maxEventAge: TimeSpan.FromDays(30));
                AddItem(instances, typeof(IAuditEventStorage), new object[] {
                    new FlatFileAuditEventStorage(CreatePath(@"AuditEvent_History.json"),
                        maxEventAge: TimeSpan.FromDays(30), delayFirstCleanup: false, blobStorage: blobStorage)
                });
            }

            if (includeType(HCModuleType.SiteEvents))
            {
                AddItem(instances, typeof(ISiteEventStorage), new object[] {
                    new FlatFileSiteEventStorage(CreatePath(@"SiteEvent_Storage.json"),
                        maxEventAge: TimeSpan.FromDays(30), delayFirstCleanup: false)
                });
            }

            TryAddExternalInstances(HCModuleType.EndpointControl, instances, includeType);
            TryAddExternalInstances(HCModuleType.Code, instances, includeType);
            TryAddExternalInstances(HCModuleType.RequestLog, instances, includeType);

            return instances
                .Where(x => x.Key != null && x.Value?.Any() == true)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private void TryAddExternalInstances(HCModuleType moduleType, Dictionary<Type, List<object>> instances, Func<HCModuleType, bool> includeType)
        {
            if (includeType(moduleType)
                && ExternalModuleInstanceFactories.TryGetValue(moduleType, out var extInstanceFactory))
            {
                var items = extInstanceFactory?.Invoke(CreatePath);
                if (items?.Any() == true)
                {
                    foreach(var item in items)
                    {
                        AddItem(instances, item.Key, item.Value);
                    }
                }
            }
        }

        private void AddItem(Dictionary<Type, List<object>> list, Type type, IEnumerable<object> items)
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
}
