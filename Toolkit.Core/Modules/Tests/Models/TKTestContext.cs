using QoDL.Toolkit.Core.Modules.Tests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QoDL.Toolkit.Core.Modules.Tests.Models
{
    /// <summary>
    /// Context object for runtime tests.
    /// <para>Static methods only have any effect when run in the context of a runtime test.</para>
    /// </summary>
    public class TKTestContext
    {
        internal DateTimeOffset TestExecutionStartTime { get; set; }
        internal List<TKTestTiming> Timings { get; private set; } = new();
        internal List<Action<TestResult>> TestResultActions { get; private set; } = new();

        private readonly object _timingsLock = new();
        private readonly object _logBuilderLock = new();
        private readonly StringBuilder _logBuilder = new();

        /// <summary>
        /// Formats the given log entry given to <see cref="Log"/>
        /// </summary>
        protected virtual string FormatLogText(string text) => $"[{DateTimeOffset.Now:HH:mm:ss:fff}]\t{text}";

        internal bool HasLoggedAnything() => _logBuilder?.Length > 0;
        internal string GetLog() => _logBuilder?.ToString()?.Trim();

        internal void EndAllTimers()
        {
            lock (_timingsLock)
            {
                foreach (var timing in Timings)
                {
                    timing.EndTimer();
                }
            }
        }

        /// <summary>
        /// Modifies the result of the currently running test in the request if any.
        /// </summary>
        public static void WithCurrentResult(Action<TestResult> action)
            => WithCurrentContext(c => c.TestResultActions.Add(action));

        /// <summary>
        /// Log the given text along with the test result output.
        /// </summary>
        public static void Log(string text) => WithCurrentContext(c =>
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                lock (c._logBuilderLock)
                {
                    c._logBuilder.AppendLine(c.FormatLogText(text));
                }
            }
        });

        /// <summary>
        /// Add timing data with a given duration.
        /// </summary>
        public static void AddTiming(string description, TimeSpan duration)
            => WithCurrentContext(c => {
                lock (c._timingsLock)
                {
                    c.Timings.Add(new TKTestTiming(description, duration, DateTimeOffset.Now - c.TestExecutionStartTime - duration));
                }
            });

        /// <summary>
        /// Start timing data with a given id.
        /// </summary>
        public static void StartTiming(string id, string description)
            => WithCurrentContext(c =>
            {
                lock (c._timingsLock)
                {
                    c.Timings.Add(new TKTestTiming(id, description, (DateTimeOffset.Now - c.TestExecutionStartTime)));
                }
            });

        /// <summary>
        /// Start timing data.
        /// </summary>
        public static void StartTiming(string description)
            => WithCurrentContext(c =>
            {
                lock(c._timingsLock)
                {
                    c.Timings.Add(new TKTestTiming(Guid.NewGuid().ToString(), description, (DateTimeOffset.Now - c.TestExecutionStartTime)));
                }
            });

        /// <summary>
        /// End a timing, if given id is null the latest timing will be stopped.
        /// </summary>
        /// <param name="id">Id of timing to stop, if null the latest timing will be stopped.</param>
        public static void EndTiming(string id = null)
            => WithCurrentContext(c =>
            {
                lock(c._timingsLock)
                {
                    var timing = c.Timings.LastOrDefault(x => id == null || x.Id == id);
                    timing?.EndTimer();
                }
            });

        /// <summary>
        /// End all timings that has not already been stopped.
        /// </summary>
        public static void EndAllTimings() => WithCurrentContext(c => c.EndAllTimers());

        /// <summary>
        /// Only executes the given action when the request is executing a test.
        /// </summary>
        public static void WithCurrentContext(Action<TKTestContext> action)
        {
            try
            {
                if (TestRunnerService.CurrentRequestIsTest?.Invoke() != true || action == null)
                {
                    return;
                }

                var context = TestRunnerService.GetCurrentTestContext();
                if (context == null)
                {
                    return;
                }

                action(context);
            }
            catch(Exception)
            {
                /* ignored */
            }
        }

        /// <summary>
        /// Only executes the given action when the request is executing a test.
        /// <para>If the request is not a testrun, default will be returned.</para>
        /// </summary>
        public static T GetFromCurrentContext<T>(Func<TKTestContext, T> action)
        {
            try
            {
                if (TestRunnerService.CurrentRequestIsTest?.Invoke() != true || action == null)
                {
                    return default;
                }

                var context = TestRunnerService.GetCurrentTestContext();
                if (context == null)
                {
                    return default;
                }

                return action(context);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
