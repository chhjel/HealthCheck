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
    public class SimpleDataStore<TItem>
    {
        /// <summary>
        /// Path to the storage file.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Writes will be queued up for this duration.
        /// Defaults to 2 seconds.
        /// </summary>
        public float WriteDelay { get; set; } = 2f;

        internal Func<TItem, string[]> Serializer { get; set; }
        internal Func<string[], TItem> Deserializer { get; set; }

        internal bool IsWriteQueued { get; set; }
        internal List<string> NewRowsBuffer { get; set; } = new List<string>();
        internal readonly object _fileLock = new object();

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
            Serializer = serializer;
            Deserializer = deserializer;

            EnsureFileExists();
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
            Task.Run(() => QueueWriteBufferToFile(row));
            return item;
        }

        /// <summary>
        /// Delete all rows that match the given condition.
        /// </summary>
        public void DeleteWhere(Func<TItem, bool> condition)
        {
            lock (NewRowsBuffer)
            {
                NewRowsBuffer.RemoveAll(x => CheckCondition(x, condition, true));
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
        public void UpdateWhere(Func<TItem, bool> condition, Func<TItem, TItem> update)
        {
            lock (NewRowsBuffer)
            {
                for (int i = 0; i < NewRowsBuffer.Count; i++)
                {
                    var item = DeserializeRow(NewRowsBuffer[i]);
                    if (item != null && condition(item))
                    {
                        NewRowsBuffer[i] = SerializeItem(update(item));
                    }
                }
            }

            EnsureFileExists();
            lock (_fileLock)
            {
                var tempFile = Path.GetTempFileName();

                var updatedLines = File.ReadLines(FilePath)
                    .Select(row => new { row, item = DeserializeRow(row) })
                    .Select(x =>
                        (x.item != null && condition(x.item))
                            ? SerializeItem(update(x.item))
                            : x.row
                    );

                File.WriteAllLines(tempFile, updatedLines);

                File.Delete(FilePath);
                File.Move(tempFile, FilePath);
                OnFileWrittenEvent?.Invoke();
            }
        }

        /// <summary>
        /// Clear the whole storage file.
        /// </summary>
        public async Task ClearDataAsync()
        {
            lock (NewRowsBuffer)
            {
                NewRowsBuffer.Clear();
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
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }
        }

        /// <summary>
        /// Get all rows as enumerable, reading one line at a time. Also reads buffered lines.
        /// </summary>
        /// <param name="fromEnd">Read from the end of file.</param>
        public IEnumerable<TItem> GetEnumerable(bool fromEnd = false)
        {
            EnsureFileExists();

            if (fromEnd)
            {
                lock (NewRowsBuffer)
                {
                    foreach (var bufferedRow in NewRowsBuffer.Reverse<string>())
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
                    using (FileStream fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (ReverseStreamReader streamReader = new ReverseStreamReader(fileStream))
                    {
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
            }
            else
            {
                lock (_fileLock)
                {
                    using (FileStream fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (BufferedStream bufferedStream = new BufferedStream(fileStream))
                    using (StreamReader streamReader = new StreamReader(bufferedStream))
                    {
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

                lock (NewRowsBuffer)
                {
                    foreach (var bufferedRow in NewRowsBuffer)
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

        private async Task QueueWriteBufferToFile(string row)
        {
            // Store text in memory
            lock (NewRowsBuffer)
            {
                NewRowsBuffer.Add(row);

                // Return if already queued a write
                if (IsWriteQueued)
                {
                    return;
                }
                IsWriteQueued = true;
            }

            // Wait to write
            await Task.Delay(TimeSpan.FromSeconds(WriteDelay));

            // Write and clear queue
            WriteBufferToFile();
        }

        internal void WriteBufferToFile()
        {
            lock (NewRowsBuffer)
            {
                if (NewRowsBuffer.Count == 0) return;

                try
                {
                    EnsureFileExists();
                    lock (_fileLock)
                    {
                        File.AppendAllLines(FilePath, NewRowsBuffer);
                        OnFileWrittenEvent?.Invoke();
                    }
                    NewRowsBuffer.Clear();
                }
                catch (Exception)
                {
                    // Retry on next write. Very low chance of exception being thrown here.
                }
                IsWriteQueued = false;
            }
        }

        private TItem DeserializeRow(string row) => Deserializer(Decode(row));
        private string SerializeItem(TItem item) => Encode(Serializer(item));

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

        private readonly Regex _partSplitRegex = new Regex(@"(?<=[^\\])\|");
        private readonly Regex _newlineRegex = new Regex(@"(?<!\\)(?:\\{2})*\\n");
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
    }
}
