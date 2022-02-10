using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Util.Collections;
using HealthCheck.WebUI.Serializers;
using HealthCheck.WebUI.Util;
using System;

#if NETFRAMEWORK
using System.Web;
using System.Web.Mvc;
#endif

#if NETCORE
using HealthCheck.Core.Util;
using Microsoft.AspNetCore.Http;
#endif

namespace HealthCheck.WebUI.Config
{
    internal class ConfigInitializer : IHCExtModuleInitializer
    {
        /// <summary>
        /// Invoked from <see cref="HCGlobalConfig"/> constructor.
        /// </summary>
        public void Initialize()
        {
            if (HCGlobalConfig.DefaultInstanceResolver == null)
            {
                SetDefaultInstanceResolver();
            }
            if (HCMetricsUtil.CurrentContextFactory == null)
            {
                SetDefaultHCMetricsContextFactory();
            }
            if (HCGlobalConfig.RequestContextFactory == null)
            {
                HCGlobalConfig.RequestContextFactory = HCRequestContextFactory.Create;
            }
            if (HCGlobalConfig.Serializer == null)
            {
                HCGlobalConfig.Serializer = new NewtonsoftJsonSerializer();
            }
            if (HCGlobalConfig.GetCurrentSessionId == null)
            {
#if NETFRAMEWORK
                HCGlobalConfig.GetCurrentSessionId = () => HttpContext.Current?.Session?.SessionID;
#elif NETCORE
                HCGlobalConfig.GetCurrentSessionId = () => { try { return HCIoCUtils.GetInstance<IHttpContextAccessor>()?.HttpContext?.Session?.Id; } catch (Exception) { return null; } };
#endif
            }
        }

        private static void SetDefaultHCMetricsContextFactory()
        {
#if NETFRAMEWORK || NETCORE
            HCMetricsUtil.CurrentContextFactory = DefaultHCMetricsContextFactory;
#else
            // Nothing needed otherwise
            _ = nameof(DefaultHCMetricsContextFactory);
#endif
        }

#if NETFRAMEWORK || NETCORE
        private const string _hcMetricsItemsContextKey = "___hc_metrics_context";
        private const string _hcMetricsItemsAllowedKey = "___hc_metrics_context_allowed";
#endif
        private static readonly HCDelayedAutoDisposer<HCMetricsContext> _staticMetricsContext = new(TimeSpan.FromSeconds(5));
        private static HCMetricsContext DefaultHCMetricsContextFactory()
        {
#if NETFRAMEWORK
            var context = HttpContext.Current;
            var items = context?.Items;
            if (items != null)
            {
                if (!items.Contains(_hcMetricsItemsAllowedKey))
                {
                    var hcContext = HCGlobalConfig.RequestContextFactory?.Invoke();
                    var allowed = hcContext != null && HCMetricsUtil.AllowTrackRequestMetrics?.Invoke(hcContext) == true;
                    items[_hcMetricsItemsAllowedKey] = allowed;
                    if (!allowed)
                    {
                        return null;
                    }
                }
                else if (items[_hcMetricsItemsAllowedKey] is not bool allowed || !allowed)
                {
                    return null;
                }

                if (items[_hcMetricsItemsContextKey] == null)
                {
                    items[_hcMetricsItemsContextKey] = new HCMetricsContext(context?.Timestamp ?? DateTimeOffset.Now);
                }

                return items[_hcMetricsItemsContextKey] as HCMetricsContext;
            }
#elif NETCORE
            var context = HCIoCUtils.GetInstance<IHttpContextAccessor>();
            var items = context?.HttpContext?.Items;
            if (items != null)
            {
                if (!items.ContainsKey(_hcMetricsItemsAllowedKey))
                {
                    var hcContext = HCGlobalConfig.RequestContextFactory?.Invoke();
                    var allowed = hcContext != null && HCMetricsUtil.AllowTrackRequestMetrics?.Invoke(hcContext) == true;
                    items[_hcMetricsItemsAllowedKey] = allowed;
                    if (!allowed)
                    {
                        return null;
                    }
                }
                else if (items[_hcMetricsItemsAllowedKey] is not bool allowed || !allowed)
                {
                    return null;
                }

                if (items[_hcMetricsItemsContextKey] == null)
                {
                    items[_hcMetricsItemsContextKey] = new HCMetricsContext(DateTimeOffset.Now);
                }

                return items[_hcMetricsItemsContextKey] as HCMetricsContext;
            }
#endif

            // Request.Items is null at this point, so fallback to a static context that is disposed a few seconds after each usage.
            var requestContext = HCGlobalConfig.RequestContextFactory?.Invoke();
            if (HCMetricsUtil.AllowTrackRequestMetrics?.Invoke(requestContext) == true)
            {
                return _staticMetricsContext?.Value;
            }

            return null;
        }

        private static void SetDefaultInstanceResolver()
        {
            /* No effect for other targets atm. */
#if NETFRAMEWORK
            HCGlobalConfig.DefaultInstanceResolver = (type) => DependencyResolver.Current.GetService(type);
#endif
        }
    }
}
