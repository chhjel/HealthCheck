using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Linq;

namespace HealthCheck.Core.Modules.DataRepeater.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IHCDataRepeaterStreamItem"/>.
    /// </summary>
    public static class IHCDataRepeaterStreamItemExtensions
    {
        /// <summary>
        /// Adds a new log message and optionally limits max log size.
        /// <para>Does not save anything.</para>
        /// </summary>
        public static void AddLogMessage(this IHCDataRepeaterStreamItem item, string message, int maxLogSize = 100)
        {
            item.Log ??= new();
            item.Log.Add(new HCDataRepeaterSimpleLogEntry
            {
                Timestamp = DateTimeOffset.Now,
                Message = message
            });
            if (item.Log.Count > maxLogSize)
            {
                item.Log = item.Log.Skip(item.Log.Count - maxLogSize).Take(maxLogSize).ToList();
            }
        }
    }
}
