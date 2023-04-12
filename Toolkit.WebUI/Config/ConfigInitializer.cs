using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Core.Util.Collections;
using QoDL.Toolkit.WebUI.Serializers;
using QoDL.Toolkit.WebUI.Util;
using System;

#if NETFRAMEWORK
using System.Web;
using System.Web.Mvc;
#endif

#if NETCORE
using QoDL.Toolkit.Core.Util;
using Microsoft.AspNetCore.Http;
#endif

namespace QoDL.Toolkit.WebUI.Config;

internal class ConfigInitializer : ITKExtModuleInitializer
{
    /// <summary>
    /// Invoked from <see cref="TKGlobalConfig"/> constructor.
    /// </summary>
    public void Initialize()
    {
        if (TKGlobalConfig.DefaultInstanceResolver == null)
        {
            SetDefaultInstanceResolver();
        }
        if (TKMetricsUtil.CurrentContextFactory == null)
        {
            SetDefaultTKMetricsContextFactory();
        }
        TKGlobalConfig.RequestContextFactory ??= TKRequestContextFactory.Create;
        TKGlobalConfig.Serializer ??= new NewtonsoftJsonSerializer();
        if (TKGlobalConfig.GetCurrentSessionId == null)
        {
#if NETFRAMEWORK
            TKGlobalConfig.GetCurrentSessionId = () => HttpContext.Current?.Session?.SessionID;
#elif NETCORE
            TKGlobalConfig.GetCurrentSessionId = () => { try { return TKIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext?.Session?.Id; } catch (Exception) { return null; } };
#endif
        }
    }

    private static void SetDefaultTKMetricsContextFactory()
    {
#if NETFRAMEWORK || NETCORE
        TKMetricsUtil.CurrentContextFactory = DefaultTKMetricsContextFactory;
#else
        // Nothing needed otherwise
        _ = nameof(DefaultTKMetricsContextFactory);
#endif
    }

#if NETFRAMEWORK || NETCORE
    private const string _tkMetricsItemsContextKey = "___tk_metrics_context";
    private const string _tkMetricsItemsAllowedKey = "___tk_metrics_context_allowed";
#endif
    private static readonly TKDelayedAutoDisposer<TKMetricsContext> _staticMetricsContext = new(TimeSpan.FromSeconds(5));
    private static TKMetricsContext DefaultTKMetricsContextFactory()
    {
#if NETFRAMEWORK
        var context = HttpContext.Current;
        var items = context?.Items;
        if (items != null)
        {
            if (!items.Contains(_tkMetricsItemsAllowedKey))
            {
                var tkContext = TKGlobalConfig.RequestContextFactory?.Invoke();
                var allowed = tkContext != null && TKMetricsUtil.AllowTrackRequestMetrics?.Invoke(tkContext) == true;
                items[_tkMetricsItemsAllowedKey] = allowed;
                if (!allowed)
                {
                    return null;
                }
            }
            else if (items[_tkMetricsItemsAllowedKey] is not bool allowed || !allowed)
            {
                return null;
            }

            if (items[_tkMetricsItemsContextKey] == null)
            {
                items[_tkMetricsItemsContextKey] = new TKMetricsContext(context?.Timestamp ?? DateTimeOffset.Now);
            }

            return items[_tkMetricsItemsContextKey] as TKMetricsContext;
        }
#elif NETCORE
        var context = TKIoCUtils.GetInstance<IHttpContextAccessor>();
        var items = context?.HttpContext?.Items;
        if (items != null)
        {
            if (!items.ContainsKey(_tkMetricsItemsAllowedKey))
            {
                var tkContext = TKGlobalConfig.RequestContextFactory?.Invoke();
                var allowed = tkContext != null && TKMetricsUtil.AllowTrackRequestMetrics?.Invoke(tkContext) == true;
                items[_tkMetricsItemsAllowedKey] = allowed;
                if (!allowed)
                {
                    return null;
                }
            }
            else if (items[_tkMetricsItemsAllowedKey] is not bool allowed || !allowed)
            {
                return null;
            }

            if (items[_tkMetricsItemsContextKey] == null)
            {
                items[_tkMetricsItemsContextKey] = new TKMetricsContext(DateTimeOffset.Now);
            }

            return items[_tkMetricsItemsContextKey] as TKMetricsContext;
        }
#endif

        // Request.Items is null at this point, so fallback to a static context that is disposed a few seconds after each usage.
        var requestContext = TKGlobalConfig.RequestContextFactory?.Invoke();
        if (TKMetricsUtil.AllowTrackRequestMetrics?.Invoke(requestContext) == true)
        {
            return _staticMetricsContext?.Value;
        }

        return null;
    }

    private static void SetDefaultInstanceResolver()
    {
        /* No effect for other targets atm. */
#if NETFRAMEWORK
        TKGlobalConfig.DefaultInstanceResolver = (type, scopeContainer) => DependencyResolver.Current.GetService(type);
#endif
    }
}
