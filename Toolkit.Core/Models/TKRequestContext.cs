using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Models
{
    /// <summary>
    /// Shared request model for .NET Framework and .NET Core.
    /// </summary>
    public class TKRequestContext
    {
        /// <summary>
        /// True as long as we're in a request context. When e.g. created from a scheduled job this will most likely be false.
        /// </summary>
        public bool HasRequestContext { get; set; }

        /// <summary>
        /// Is set automatically to the full url of the request.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Is set automatically to all header keys and values.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Is set automatically to all request cookies.
        /// </summary>
        public Dictionary<string, string> Cookies { get; set; }

        /// <summary>
        /// POST, GET etc
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// When the request started.
        /// </summary>
        public DateTimeOffset RequestExecutionStartTime { get; set; }

        /// <summary>
        /// Logic for getting request item by key.
        /// </summary>
        internal static Func<string, object> RequestItemGetter { get; set; }

        /// <summary>
        /// Logic for setting request item by key.
        /// </summary>
        internal static Action<string, object> RequestItemSetter { get; set; }

        /// <summary>
        /// Get a request item by key.
        /// </summary>
        public static T GetRequestItem<T>(string key, T fallback)
            where T : class
        {
            try
            {
                return (RequestItemGetter?.Invoke(key) ?? fallback) as T;
            }
            catch(Exception)
            {
                return fallback;
            }
        }

        /// <summary>
        /// Set a request item by key.
        /// </summary>
        public static void SetRequestItem<T>(string key, T value)
            where T : class
        {
            try
            {
                RequestItemSetter?.Invoke(key, value);
            }
            catch (Exception) { /* Ignored */ }
        }
    }
}
