using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using System;
#if NETFULL
using System.Web.Mvc;

namespace HealthCheck.WebUI.Util
{
    /// <summary>
    /// Utilities related to the event notifications module.
    /// </summary>
    public static class EventSinkUtil
    {
        /// <summary>
        /// Register an event that can be filtered upon and notified in the healthcheck.
        /// <para>
        /// Shortcut to DependencyResolver.Current?.GetService&lt;IEventDataSink&gt;()?.RegisterEvent(eventId, payload)
        /// wrapped in a try/catch to prevent making the code a bit noisy with utility code.
        /// </para>
        /// </summary>
        public static void TryRegisterEvent(string eventId, object payload = null)
        {
            try
            {
                DependencyResolver.Current?.GetService<IEventDataSink>()?.RegisterEvent(eventId, payload);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Register an event that can be filtered upon and notified in the healthcheck.
        /// <para>
        /// Shortcut to DependencyResolver.Current?.GetService&lt;IEventDataSink&gt;()?.RegisterEvent(eventId, payload)
        /// wrapped in a try/catch to prevent making the code a bit noisy with utility code.
        /// </para>
        /// </summary>
        public static void TryRegisterEvent(string eventId, Func<object> payload = null)
        {
            try
            {
                DependencyResolver.Current?.GetService<IEventDataSink>()?.RegisterEvent(eventId, payload?.Invoke());
            }
            catch (Exception) { }
        }
    }
}
#endif
