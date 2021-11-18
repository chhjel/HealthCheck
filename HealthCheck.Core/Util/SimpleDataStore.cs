using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Modules.Metrics.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Simple data storage to flatfile.
    /// <para>Requires your own item serialization/deserialization logic.</para>
    /// <para>If you need item ids use <see cref="SimpleDataStoreWithId{TItem, TId}"/> instead.</para>
    /// </summary>
    public class SimpleDataStore<TItem> : IDataStore<TItem>
    {
        /// <summary>
        /// Options for automatic storage cleanup.
        /// </summary>
        public StorageRetentionOptions<TItem> RetentionOptions { get; set; }

        /// <summary>
        /// Path to the storage file.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Writes will be queued up for this duration.
        /// Defaults to 2 seconds.
        /// </summary>
        public float WriteDelay { get; set; } = 2f;

        internal Func<TItem, string[]> _serializer;
        internal Func<string[], TItem> _deserializer;
        internal bool _isWriteQueued;
        internal List<string> _newRowsBuffer = new();
        internal readonly object _fileLock = new();
        private DateTimeOffset? _lastCleanupPerformedAt;

        /// <summary>
        /// Signature of OnFileWrittenEvent.
        /// </summary>
        public delegate void OnFileWritten();

        /// <summary>
        /// Triggered whenever the file is updated.
        /// </summary>
        public event OnFileWritten OnFileWrittenEvent;

        /// <summary>
        /// Simple data storage to flatfile.
        /// <para>Requires your own item serialization/deserialization logic.</para>
        /// </summary>
        /// <param name="filepath">Path to the file where data will be stored.</param>
        /// <param name="serializer">Serialize the wanted properties into string columns.</param>
        /// <param name="deserializer">Deserialize the serialized columns back into their properties.</param>
        public SimpleDataStore(string filepath,
            Func<TItem, string[]> serializer,
            Func<string[], TItem> deserializer)
        {
            FilePath = filepath;
            _serializer = serializer;
            _deserializer = deserializer;

            EnsureFileExists();
        }

        /// <summary>
        /// Simple data storage to flatfile.
        /// <para>Requires your own item serialization/deserialization logic.</para>
        /// </summary>
        /// <param name="filepath">Path to the file where data will be stored.</param>
        /// <param name="serializer">Serialize the wanted properties into a string.</param>
        /// <param name="deserializer">Deserialize the serialized string back into a object.</param>
        public SimpleDataStore(string filepath,
            Func<TItem, string> serializer,
            Func<string, TItem> deserializer)
            : this(
                 filepath,
                 serializer: new Func<TItem, string[]>(item => new string[] { serializer(item) }),
                 deserializer: new Func<string[], TItem>(columns => columns.Length == 0 ? default : deserializer(columns[0]))
        )
        { }

        /// <summary>
        /// Deconstructor. Stores any buffered data before self destructing.
        /// </summary>
        ~SimpleDataStore()
        {
            WriteBufferToFile();
        }

        /// <summary>
        /// Add a new row in the file with the given object.
        /// </summary>
        public virtual TItem InsertItem(TItem item)
        {
            var row = SerializeItem(item);
            QueueWriteBufferToFile(row);
            CheckCleanup();
            return item;
        }

        /// <summary>
        /// Delete all rows that match the given condition.
        /// </summary>
        public void DeleteWhere(Func<TItem, bool> condition)
        {
            lock (_newRowsBuffer)
            {
                _newRowsBuffer.RemoveAll(x => CheckCondition(x, condition, true));
            }

            EnsureFileExists();
            lock (_fileLock)
            {
                var tempFile = Path.GetTempFileName();

                var linesToKeep = File.ReadLines(FilePath)
                    .Where(x => CheckCondition(x, (item) => !condition(item), false));
                File.WriteAllLines(tempFile, linesToKeep);

                File.Delete(FilePath);
                File.Move(tempFile, FilePath);
                OnFileWrittenEvent?.Invoke();
                HCMetricsContext.IncrementGlobalCounter($"{GetType().Name}<{typeof(TItem).Name}>.LoadData()", 1);
                HCMetricsContext.IncrementGlobalCounter($"{GetType().Name}<{typeof(TItem).Name}>.SaveData()", 1);
            }
        }

        private bool CheckCondition(string row, Func<TItem, bool> condition, bool valueIfnull)
        {
            var item = DeserializeRow(row);
            if (item == null) return valueIfnull;
            else return condition(item);
        }

        /// <summary>
        /// Perform the given changes on all rows that match the given condition.
        /// </summary>
        public int UpdateWhere(Func<TItem, bool> condition, Func<TItem, TItem> update)
            => UpdateWhereInternal(condition, update);

        /// <summary>
        /// Perform the given changes on all rows that match the given condition.
        /// </summary>
        internal int UpdateWhereInternal(Func<TItem, bool> condition, Func<TItem, TItem> update, IEnumerable<string> mustContainAny = null)
        {
            bool mustContainCheck(string line)
            {
                return mustContainAny == null || mustContainAny.Any(x => line.Contains(x));
            }

            int matchCount = 0;
            lock (_newRowsBuffer)
            {
                for (int i = 0; i < _newRowsBuffer.Count; i++)
                {
                    if (!mustContainCheck(_newRowsBuffer[i]))
                    {
                        continue;
                    }

                    var item = DeserializeRow(_newRowsBuffer[i]);
                    if (item != null && condition(item))
                    {
                        matchCount++;
                        _newRowsBuffer[i] = SerializeItem(update(item));
                    }
                }
            }

            EnsureFileExists();
            lock (_fileLock)
            {
                var tempFile = Path.GetTempFileName();

                var updatedLines = File.ReadLines(FilePath)
                    .Select(row => new { row, item = mustContainCheck(row) ? new Maybe<TItem>(DeserializeRow(row)) : null })
                    .Select(x =>
                    {
                        if (x.item != null && x.item.HasValue && condition(x.item.Value))
                        {
                            matchCount++;
                            return SerializeItem(update(x.item.Value));
                        }
                        return x.row;
                    });

                File.WriteAllLines(tempFile, updatedLines);

                File.Delete(FilePath);
                File.Move(tempFile, FilePath);
                OnFileWrittenEvent?.Invoke();
                HCMetricsContext.IncrementGlobalCounter($"{GetType().Name}<{typeof(TItem).Name}>.LoadData()", 1);
                HCMetricsContext.IncrementGlobalCounter($"{GetType().Name}<{typeof(TItem).Name}>.SaveData()", 1);
            }

            return matchCount;
        }

        /// <summary>
        /// Clear the whole storage file.
        /// </summary>
        public async Task ClearDataAsync()
        {
            lock (_newRowsBuffer)
            {
                _newRowsBuffer.Clear();
            }

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    EnsureFileExists();
                    lock (_fileLock)
                    {
                        File.WriteAllText(FilePath, string.Empty);
                        OnFileWrittenEvent?.Invoke();
                    }
                    break;
                }
                catch (Exception)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                }
            }
        }

        /// <summary>
        /// Get all rows as enumerable, reading one line at a time. Also reads buffered lines.
        /// </summary>
        public IEnumerable<TItem> GetEnumerable()
            => GetEnumerable(fromEnd: false);

        /// <summary>
        /// Get all rows as enumerable, reading one line at a time. Also reads buffered lines.
        /// </summary>
        /// <param name="fromEnd">Read from the end of file.</param>
        public IEnumerable<TItem> GetEnumerable(bool fromEnd = false)
        {
            EnsureFileExists();

            if (fromEnd)
            {
                lock (_newRowsBuffer)
                {
                    foreach (var bufferedRow in _newRowsBuffer.Reverse<string>())
                    {
                        if (string.IsNullOrWhiteSpace(bufferedRow))
                            continue;

                        var item = DeserializeRow(bufferedRow);
                        if (item != null)
                        {
                            yield return item;
                        }
                    }
                }

                lock (_fileLock)
                {
                    HCMetricsContext.IncrementGlobalCounter($"{GetType().Name}<{typeof(TItem).Name}>.LoadData()", 1);
                    var fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using ReverseStreamReader streamReader = new(fileStream);
                    string row;
                    while ((row = streamReader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(row))
                            continue;

                        var item = DeserializeRow(row);
                        if (item != null)
                        {
                            yield return item;
                        }
                    }
                }
            }
            else
            {
                lock (_fileLock)
                {
                    HCMetricsContext.IncrementGlobalCounter($"{GetType().Name}<{typeof(TItem).Name}>.LoadData()", 1);
                    var fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var bufferedStream = new BufferedStream(fileStream);
                    using StreamReader streamReader = new(bufferedStream);
                    string row;
                    while ((row = streamReader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(row))
                            continue;

                        var item = DeserializeRow(row);
                        if (item != null)
                        {
                            yield return item;
                        }
                    }
                }

                lock (_newRowsBuffer)
                {
                    foreach (var bufferedRow in _newRowsBuffer)
                    {
                        if (string.IsNullOrWhiteSpace(bufferedRow))
                            continue;

                        var item = DeserializeRow(bufferedRow);
                        if (item != null)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Perform cleanup if retention options allow it.
        /// </summary>
        protected void CheckCleanup()
        {
            if (_lastCleanupPerformedAt == null && RetentionOptions?.DelayFirstCleanupByMinimumCleanupInterval == true)
            {
                _lastCleanupPerformedAt = DateTimeOffset.Now;
            }

            if (!CanCleanup())
            {
                return;
            }

            PerformCleanup();
        }

        private bool CanCleanup()
        {
            // Cleanup not enabled => abort
            if (RetentionOptions == null || RetentionOptions.ItemTimestampSelector == null)
            {
                return false;
            }

            // Less than min time since last cleanup => abort
            if (_lastCleanupPerformedAt != null && (DateTimeOffset.Now.ToUniversalTime() - _lastCleanupPerformedAt?.ToUniversalTime()) < RetentionOptions.MinimumCleanupInterval)
            {
                return false;
            }

            return true;
        }

        private void PerformCleanup()
        {
            _lastCleanupPerformedAt = DateTimeOffset.Now;
            if (RetentionOptions?.MaxItemAge != null && RetentionOptions.ItemTimestampSelector != null)
            {
                var threshold = DateTimeOffset.Now.ToUniversalTime() - RetentionOptions.MaxItemAge;
                DeleteWhere(x => RetentionOptions.ItemTimestampSelector(x).ToUniversalTime() <= threshold);
            }
        }

        private void QueueWriteBufferToFile(string row)
        {
            // Store text in memory
            lock (_newRowsBuffer)
            {
                _newRowsBuffer.Add(row);

                // Return if already queued a write
                if (_isWriteQueued)
                {
                    return;
                }
                _isWriteQueued = true;
            }

            Task.Run(() => QueueWriteBufferToFileAsync());
        }

        private async Task QueueWriteBufferToFileAsync()
        {
            // Wait to write
            await Task.Delay(TimeSpan.FromSeconds(WriteDelay));

            // Write and clear queue
            WriteBufferToFile();
        }

        internal void WriteBufferToFile()
        {
            lock (_newRowsBuffer)
            {
                if (_newRowsBuffer.Count == 0)
                {
                    _isWriteQueued = false;
                    return;
                }

                try
                {
                    EnsureFileExists();
                    lock (_fileLock)
                    {
                        File.AppendAllLines(FilePath, _newRowsBuffer);
                        OnFileWrittenEvent?.Invoke();
                    }
                    _newRowsBuffer.Clear();
                }
                catch (Exception)
                {
                    // Retry on next write. Very low chance of exception being thrown here.
                }
                _isWriteQueued = false;
            }
        }

        private TItem DeserializeRow(string row) => _deserializer(Decode(row));
        private string SerializeItem(TItem item) => Encode(_serializer(item));

        private const string Delimiter = "|";
        private const string NewlineReplacement = @"\n";
        private string Encode(string[] values)
        {
            var encodedValues = values.Select(x => x
                .Replace(@"\", @"\\")
                .Replace(Delimiter, $@"\{Delimiter}")
                .Replace("\n", NewlineReplacement)
            );
            return string.Join(Delimiter, encodedValues);
        }

        private readonly Regex _partSplitRegex = new(@"(?<=[^\\])\|");
        private readonly Regex _newlineRegex = new(@"(?<!\\)(?:\\{2})*\\n");
        private string[] Decode(string encoded)
        {
            return _partSplitRegex
                .Split(encoded)
                .Select(x => _newlineRegex.Replace(x, "\n"))
                .Select(x => x
                    .Replace($@"\{Delimiter}", Delimiter)
                    .Replace(@"\\", @"\")
                ).ToArray();
        }

        private void EnsureFileExists()
        {
            lock (_fileLock)
            {
                if (!File.Exists(FilePath))
                {
                    var parentFolder = Directory.GetParent(FilePath).FullName;
                    if (!Directory.Exists(parentFolder))
                    {
                        Directory.CreateDirectory(parentFolder);
                    }
                    File.WriteAllText(FilePath, "");
                    OnFileWrittenEvent?.Invoke();
                }
            }
        }
    }
}
