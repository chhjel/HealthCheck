using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Util;
using System;

namespace QoDL.Toolkit.WebUI.Util
{
    /// <summary>
    /// Utilities related to the event notifications module.
    /// </summary>
    public static class EventSinkUtil
    {
        /// <summary>
        /// Register an event that can be filtered upon and notified in the toolkit.
        /// <para>
        /// Shortcut to DependencyResolver.Current?.GetService&lt;IEventDataSink&gt;()?.RegisterEvent(eventId, payload)
        /// wrapped in a try/catch to prevent making the code a bit noisy with utility code.
        /// </para>
        /// </summary>
        public static void TryRegisterEvent(string eventId, object payload = null)
        {
            try
            {
                TKIoCUtils.GetInstance<IEventDataSink>()?.RegisterEvent(eventId, payload);
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(EventSinkUtil), nameof(TryRegisterEvent), ex);
            }
        }

        /// <summary>
        /// Register an event that can be filtered upon and notified in the toolkit.
        /// <para>
        /// Shortcut to DependencyResolver.Current?.GetService&lt;IEventDataSink&gt;()?.RegisterEvent(eventId, payload)
        /// wrapped in a try/catch to prevent making the code a bit noisy with utility code.
        /// </para>
        /// </summary>
        public static void TryRegisterEvent(string eventId, Func<object> payload = null)
        {
            try
            {
                TKIoCUtils.GetInstance<IEventDataSink>()?.RegisterEvent(eventId, payload?.Invoke());
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(EventSinkUtil), nameof(TryRegisterEvent), ex);
            }
        }
    }
}
