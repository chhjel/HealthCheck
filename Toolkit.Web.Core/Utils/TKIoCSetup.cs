using QoDL.Toolkit.Core.Config;
#if NETCORE
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using QoDL.Toolkit.Core.Exceptions;
using QoDL.Toolkit.Core.Util;
#endif

namespace QoDL.Toolkit.Web.Core.Utils;

/// <summary>
/// Shortcuts for setting up <see cref="TKGlobalConfig.DefaultInstanceResolver"/> with different providers.
/// </summary>
public static class TKIoCSetup
{
#if NETCORE
    private static IList<ServiceDescriptor>? _serviceDescriptorCache;
    private static readonly ConcurrentDictionary<Type, bool> _scopedServiceDefCache = new();

    /// <summary>
    /// Setup for <c>Microsoft.Extensions.DependencyInjection</c>.
    /// </summary>
    public static void ConfigureForServiceProvider(IServiceProvider serviceProvider)
    {
        TKGlobalConfig.DefaultInstanceResolver = (type, currentScopeContainer) =>
        {
            try
            {
                if (_serviceDescriptorCache == null)
                {
                    _serviceDescriptorCache = TKReflectionUtils.TryGetMemberValue(TKReflectionUtils.TryGetMemberValue(serviceProvider, "CallSiteFactory"), "_descriptors") as IList<ServiceDescriptor> ?? new List<ServiceDescriptor>();
                    if (_serviceDescriptorCache == null) throw new TKException("Failed to resolve service descriptors. Configure TKGlobalConfig.DefaultInstanceResolver = .. manually.");
                }

                if (!_scopedServiceDefCache.ContainsKey(type))
                {
                    var serviceDescriptor = _serviceDescriptorCache.FirstOrDefault(x => x.ServiceType == type);
                    _scopedServiceDefCache[type] = serviceDescriptor?.Lifetime == ServiceLifetime.Scoped;
                }

                var requiresScope = _scopedServiceDefCache[type];
                if (requiresScope)
                {
                    var scope = currentScopeContainer?.Scope as IServiceScope ?? serviceProvider.CreateScope();
                    if (currentScopeContainer != null) currentScopeContainer.Scope = scope;
                    return scope.ServiceProvider.GetService(type);
                }

                return serviceProvider.GetService(type);
            }
            catch (InvalidOperationException)
            {
                var scope = serviceProvider.CreateScope();
                return scope.ServiceProvider.GetService(type);
            }
        };
    }
#endif
}
