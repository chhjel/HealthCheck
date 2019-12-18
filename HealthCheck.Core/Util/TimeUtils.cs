using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Utilities related to time and durations.
    /// </summary>
    public class TimeUtils
    {
        /// <summary>
        /// Translates the given number of milliseconds into text.
        /// <para>E.g: "2 seconds" or "9minutes, 15 seconds"</para>
        /// </summary>
        public static string PrettifyDuration(long ms, string zero = "0 milliseconds")
        {
            var timeSpan = TimeSpan.FromMilliseconds(ms);
            Func<Tuple<int, string>, string> numberFormatter = t => t.Item1 + " " + t.Item2 + (t.Item1 == 1 ? string.Empty : "s");
            var components = new List<Tuple<int, string>>
            {
                Tuple.Create((int) timeSpan.TotalDays, "day"),
                Tuple.Create(timeSpan.Hours, "hour"),
                Tuple.Create(timeSpan.Minutes, "minute"),
                Tuple.Create(timeSpan.Seconds, "second"),
                Tuple.Create(timeSpan.Milliseconds, "millisecond"),
            };

            var relevantComponents = new List<Tuple<int, string>>();
            var foundFirst = false;
            for (int i = 0; i < components.Count; i++)
            {
                var c = components[i];
                if (c.Item1 > 0 && !foundFirst)
                {
                    relevantComponents.Add(c);
                    foundFirst = true;
                }
                else if (foundFirst)
                {
                    if (c.Item1 > 0 && c.Item2 != "millisecond")
                        relevantComponents.Add(c);
                    break;
                }
            }

            var duration = string.Join(", ", relevantComponents.Select(numberFormatter));
            if (duration.Trim().Length > 0)
                return duration;
            else
                return zero;
        }
    }
}
