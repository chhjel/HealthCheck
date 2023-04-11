using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using QoDL.Toolkit.Core.Modules.DataRepeater.Utils;
using System;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ITKDataRepeaterStreamItem"/>.
    /// </summary>
    public static class ITKDataRepeaterStreamItemExtensions
    {
        /// <summary>
        /// Adds a new log message and optionally limits max log size.
        /// <para>If no max log size is provided the default value from <see cref="TKDataRepeaterUtils.DefaultMaxItemLogEntries"/> will be used.</para>
        /// <para>Does not save anything.</para>
        /// </summary>
        public static void AddLogMessage(this ITKDataRepeaterStreamItem item, string message, int? maxLogSize = null)
        {
            maxLogSize ??= TKDataRepeaterUtils.DefaultMaxItemLogEntries;

            item.Log ??= new();
            item.Log.Add(new TKDataRepeaterSimpleLogEntry
            {
                Timestamp = DateTimeOffset.Now,
                Message = message
            });
            if (item.Log.Count > maxLogSize)
            {
                item.Log = item.Log.Skip(item.Log.Count - maxLogSize.Value).Take(maxLogSize.Value).ToList();
            }
        }
    }
}
