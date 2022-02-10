using HealthCheck.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Utility used for gathering some data during a request using the request items collection.
    /// </summary>
    public class HCRequestData
    {
        private const string _requestItemKey = "___hc_request_data";

        /// <summary>
        /// Any custom details added through <see cref="SetDetail"/>
        /// </summary>
        public Dictionary<string, string> Details { get; private set; } = new();

        /// <summary>
        /// True if there's any details.
        /// </summary>
        public bool HasDetails => Details.Any();

        /// <summary>
        /// Any custom counters added through <see cref="IncrementCounter"/>
        /// </summary>
        public Dictionary<string, long> Counters { get; private set; } = new();

        /// <summary>
        /// True if there's any counters.
        /// </summary>
        public bool HasCounters => Counters.Any();

        /// <summary>
        /// Any errors added through <see cref="AddError(string, string)"/> or <see cref="AddError(string, Exception)"/>.
        /// </summary>
        public List<HCRequestDataErrorDetails> Errors { get; private set; } = new();

        /// <summary>
        /// True if there's any errors.
        /// </summary>
        public bool HasErrors => Errors.Any();

        /// <summary>
        /// True if any data has been added to the object.
        /// </summary>
        public bool ContainsData => Details.Any() || Counters.Any() || Errors.Any();

        /// <summary>
        /// Retrieves the details as a single string with the given header if there's any details.
        /// </summary>
        public string GetDetails(string header = null, string prefix = "* ", Func<string, bool> keyCondition = null)
        {
            var builder = new StringBuilder();
            var details = Details.Where(x => keyCondition?.Invoke(x.Key) != false).ToArray();
            if (details.Any())
            {
                if (!string.IsNullOrWhiteSpace(header))
                {
                    builder.AppendLine(header);
                }
                builder.AppendLine(string.Join(Environment.NewLine, details.Select(x => $"{prefix}{x.Key}: {x.Value}")));
            }
            return builder.ToString().Trim();
        }

        /// <summary>
        /// Retrieves the counters as a single string with the given header if there's any details.
        /// </summary>
        public string GetCounters(string header = null, string prefix = "* ")
        {
            var builder = new StringBuilder();
            if (Counters.Any())
            {
                if (!string.IsNullOrWhiteSpace(header))
                {
                    builder.AppendLine(header);
                }
                builder.AppendLine(string.Join(Environment.NewLine, Counters.Select(x => $"{prefix}{x.Key}: {x.Value}")));
            }
            return builder.ToString().Trim();
        }

        /// <summary>
        /// Retrieves the errors as a single string with the given header if there's any details.
        /// </summary>
        public string GetErrors(string header = null, string prefix = "* ")
        {
            var builder = new StringBuilder();
            if (Errors.Any())
            {
                if (!string.IsNullOrWhiteSpace(header))
                {
                    builder.AppendLine(header);
                }
                builder.AppendLine(string.Join(Environment.NewLine, Errors.Select(x =>
                {
                    return string.IsNullOrWhiteSpace(x.Details) ? $"{prefix}{x.Error}" : $"{prefix}{x.Error}\n\n{x.Details}";
                })));
            }
            return builder.ToString().Trim();
        }

        /// <summary>
        /// Creates a summary of the data if any.
        /// </summary>
        public override string ToString()
        {
            if (!ContainsData)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            if (Details.Any())
            {
                builder.AppendLine();
                builder.AppendLine(GetDetails("Details:"));
            }

            if (Counters.Any())
            {
                builder.AppendLine();
                builder.AppendLine(GetCounters("Counters:"));
            }

            if (Errors.Any())
            {
                builder.AppendLine();
                builder.AppendLine(GetErrors("Errors:"));
            }

            return builder.ToString().Trim();
        }

        /// <summary>
        /// Sets a new detail value by key.
        /// </summary>
        public static void SetDetail(string key, string detail, bool onlyIfNotNullOrEmpty = false)
        {
            if (!onlyIfNotNullOrEmpty || !string.IsNullOrWhiteSpace(detail))
            {
                UpdateCurrentRequestData(x => x.Details[key] = detail);
            }
        }

        /// <summary>
        /// Increments a counter by key.
        /// </summary>
        public static void IncrementCounter(string key, int amount = 1)
            => UpdateCurrentRequestData(x =>
            {
                if (!x.Counters.ContainsKey(key))
                {
                    x.Counters[key] = 0;
                }
                x.Counters[key] += amount;
            });

        /// <summary>
        /// Adds new error details.
        /// </summary>
        public static void AddError(string error, Exception ex = null)
            => UpdateCurrentRequestData(x =>
            {
                x.Errors.Add(new HCRequestDataErrorDetails
                {
                    Error = error,
                    Details = HCExceptionUtils.GetFullExceptionDetails(ex, returnNullIfNull: true)
                });
            });

        /// <summary>
        /// Adds new error details.
        /// </summary>
        public static void AddError(string error, string details)
            => UpdateCurrentRequestData(x =>
            {
                x.Errors.Add(new HCRequestDataErrorDetails
                {
                    Error = error,
                    Details = details
                });
            });

        /// <summary>
        /// Retrives the <see cref="HCRequestData"/> object for the current request.
        /// </summary>
        public static HCRequestData GetCurrentRequestData()
        {
            var data = HCRequestContext.GetRequestItem<HCRequestData>(_requestItemKey, null);
            return data ?? new HCRequestData();
        }

        private static void UpdateCurrentRequestData(Action<HCRequestData> action)
        {
            var data = GetCurrentRequestData();
            action(data);
            UpdateCurrentRequestData(data);
        }

        private static void UpdateCurrentRequestData(HCRequestData data)
            => HCRequestContext.SetRequestItem(_requestItemKey, data);

        /// <summary>
        /// Some error details.
        /// </summary>
        public class HCRequestDataErrorDetails
        {
            /// <summary>
            /// Error message.
            /// </summary>
            public string Error { get; set; }

            /// <summary>
            /// Error message.
            /// </summary>
            public string Details { get; set; }
        }
    }
}
