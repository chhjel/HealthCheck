using System.Diagnostics;

namespace QoDL.Toolkit.Core.Modules.Metrics.Context
{
    /// <summary>
    /// Helper to time things.
    /// </summary>
    public class TKMetricsTimer
    {
        /// <summary>
        /// Watch that starts when the object is created.
        /// </summary>
        public Stopwatch Stopwatch { get; }

        /// <summary>
        /// Id of this timing.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Helper to time things.
        /// </summary>
        public TKMetricsTimer(string id)
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
            Id = id;
        }

        /// <summary>
        /// Stops the internal stopwatch.
        /// </summary>
        public void StopTimer() => Stopwatch.Stop();
    }
}
