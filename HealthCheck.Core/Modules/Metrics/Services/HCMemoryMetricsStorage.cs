using HealthCheck.Core.Modules.Metrics.Abstractions;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Modules.Metrics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Metrics.Services
{
    /// <summary>
    /// Stores metrics data statically in memory.
    /// </summary>
    public class HCMemoryMetricsStorage : IHCMetricsStorage
    {
        /// <summary>
        /// Safeguard in case dynamic keys are attempted used.
        /// <para>Defaults to 1000</para>
        /// </summary>
        public int MaxDictionaryKeys { get; set; } = 1000;

        private readonly CompiledMetricsData _data = new();

        /// <inheritdoc />
        public Task<CompiledMetricsData> GetCompiledMetricsDataAsync()
        {
            lock (_data)
            {
                return Task.FromResult(_data);
            }
        }

        /// <inheritdoc />
        public Task StoreMetricDataAsync(HCMetricsContext data)
        {
            StoreCounterData(data);
            StoreValueData(data);
            StoreNotes(data);
            return Task.CompletedTask;
        }

        private void StoreNotes(HCMetricsContext data)
        {
            lock (_data.GlobalNotes)
            {
                var notes = data.Items.Where(x => 
                    x.Type == HCMetricsItem.MetricItemType.Note
                    && x.AddNoteToGlobals
                    && !string.IsNullOrWhiteSpace(x.Id)
                    && !string.IsNullOrWhiteSpace(x.Description));
                foreach (var item in notes)
                {
                    var hasKey = _data.GlobalNotes.ContainsKey(item.Id);
                    if (_data.GlobalNotes.Count >= MaxDictionaryKeys && !hasKey)
                    {
                        return;
                    }

                    _data.GlobalNotes[item.Id] = new CompiledMetricsNoteData
                    {
                        Id = item.Id,
                        Note = item.Description,
                        LastChanged = DateTimeOffset.Now
                    };
                }
            }
        }

        private void StoreCounterData(HCMetricsContext data)
        {
            lock (_data.GlobalCounters)
            {
                foreach(var kvp in data.GlobalCounters)
                {
                    var hasKey = _data.GlobalCounters.ContainsKey(kvp.Key);
                    if (_data.GlobalCounters.Count >= MaxDictionaryKeys && !hasKey)
                    {
                        return;
                    }

                    if (!hasKey)
                    {
                        _data.GlobalCounters[kvp.Key] = new() { Id = kvp.Key };
                    }
                    _data.GlobalCounters[kvp.Key].Value += kvp.Value;
                    _data.GlobalCounters[kvp.Key].LastChanged = DateTimeOffset.Now;
                }
            }
        }

        private void StoreValueData(HCMetricsContext data)
        {
            lock (_data.GlobalValues)
            {
                foreach (var kvp in data.GlobalValues)
                {
                    StoreGlobalValues(kvp.Key, kvp.Value);
                }

                foreach (var item in data.Items.Where(x => x.Type == HCMetricsItem.MetricItemType.Timing && x.AddTimingToGlobals))
                {
                    StoreGlobalValues(item.Id, new List<long> { item.DurationMilliseconds });
                }
            }
        }

        private void StoreGlobalValues(string id, IList<long> values)
        {
            if (!values?.Any() == true)
            {
                return;
            }

            var hasKey = _data.GlobalValues.ContainsKey(id);
            if (_data.GlobalValues.Count >= MaxDictionaryKeys && !hasKey)
            {
                return;
            }

            if (!hasKey)
            {
                _data.GlobalValues[id] = new() { Id = id };
            }

            var item = _data.GlobalValues[id];
            item.LastChanged = DateTimeOffset.Now;

            var min = values.Min();
            var max = values.Max();

            // First value
            if (item.ValueCount == 0)
            {
                item.Min = min;
                item.Max = max;
                item.Average = (long)values.Average();
            }
            // Not first value
            else
            {
                if (min < item.Min)
                {
                    item.Min = min;
                }
                if (min > item.Max)
                {
                    item.Max = max;
                }

                item.Average = ((item.Average * item.ValueCount) + ((long)values.Average() * values.Count)) / (item.ValueCount + values.Count);
            }
            item.ValueCount += values.Count;
        }
    }
}
