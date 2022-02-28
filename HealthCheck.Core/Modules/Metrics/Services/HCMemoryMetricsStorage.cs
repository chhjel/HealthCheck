using HealthCheck.Core.Extensions;
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

                    if (!hasKey)
                    {
                        _data.GlobalNotes[item.Id] = new() { Id = item.Id, FirstStored = DateTimeOffset.Now };
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
                        _data.GlobalCounters[kvp.Key] = new() { Id = kvp.Key, FirstStored = DateTimeOffset.Now };
                    }

                    var oldValue = _data.GlobalCounters[kvp.Key].Value;
                    var newValue = _data.GlobalCounters[kvp.Key].Value + kvp.Value;
                    if (newValue < oldValue && kvp.Value > 0)
                    {
                        newValue = long.MaxValue;
                    }
                    _data.GlobalCounters[kvp.Key].Value = newValue;

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
                    var suffix = data.GlobalValueSuffixes.ContainsKey(kvp.Key) ? data.GlobalValueSuffixes[kvp.Key] : null;
                    StoreGlobalValues(kvp.Key, kvp.Value, suffix);
                }

                foreach (var item in data.Items.Where(x => x.Type == HCMetricsItem.MetricItemType.Timing && x.AddTimingToGlobals))
                {
                    StoreGlobalValues(item.Id, new List<long> { item.DurationMilliseconds }, item.ValueSuffix);
                }
            }
        }

        private void StoreGlobalValues(string id, IList<long> values, string suffix)
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
                _data.GlobalValues[id] = new() { Id = id, FirstStored = DateTimeOffset.Now };
            }

            var item = _data.GlobalValues[id];
            item.LastChanged = DateTimeOffset.Now;
            item.Suffix = suffix ?? item.Suffix;

            var min = values.Min();
            var max = values.Max();

            // First value
            if (item.ValueCount == 0)
            {
                item.Min = min;
                item.Max = max;
                item.Average = values.AverageWithOverflowProtection();
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

                item.Average = values.AddToAverageWithOverflowProtection(item.Average, item.ValueCount);
            }
            item.ValueCount += values.Count;
        }
    }
}
