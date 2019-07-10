using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Simple data storage to flatfile.
    /// <para>Requires your own item serialization/deserialization logic.</para>
    /// </summary>
    public class SimpleDataStore<T>
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

        private Func<T, string[]> Serializer { get; set; }
        private Func<string[], T> Deserializer { get; set; }

        private bool IsWriteQueued { get; set; }
        private StringBuilder WriteBuilder { get; set; } = new StringBuilder();
        private readonly object _fileLock = new object();

        /// <summary>
        /// Simple data storage to flatfile.
        /// <para>Requires your own item serialization/deserialization logic.</para>
        /// </summary>
        /// <param name="filepath">Path to the file where data will be stored.</param>
        /// <param name="serializer">Serialize the wanted properties into string columns.</param>
        /// <param name="deserializer">Deserialize the serialized columns back into their properties.</param>
        public SimpleDataStore(string filepath,
            Func<T, string[]> serializer,
            Func<string[], T> deserializer)
        {
            FilePath = filepath;
            Serializer = serializer;
            Deserializer = deserializer;

            if (!File.Exists(FilePath))
            {
                var parentFolder = Directory.GetParent(filepath).FullName;
                if (!Directory.Exists(parentFolder))
                {
                    Directory.CreateDirectory(parentFolder);
                }
                File.WriteAllText(FilePath, "");
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
            Func<T, string> serializer,
            Func<string, T> deserializer)
            :this(
                 filepath,
                 new Func<T, string[]>(item => new string[] { serializer(item) }),
                 new Func<string[], T>(columns => columns.Length == 0 ? default : deserializer(columns[0]))
        ) { }

        /// <summary>
        /// Add a new row in the file with the given object.
        /// </summary>
        public void InsertItem(T item)
        {
            var row = SerializeItem(item);
            Task.Run(() => QueueWrite(row));
        }

        /// <summary>
        /// Delete all rows that matches the given condition.
        /// </summary>
        public void DeleteWhere(Func<T, bool> condition)
        {
            lock (_fileLock)
            {
                var tempFile = Path.GetTempFileName();

                var linesToKeep = File.ReadLines(FilePath)
                    .Where(x => !condition(DeserializeRow(x)));
                File.WriteAllLines(tempFile, linesToKeep);

                File.Delete(FilePath);
                File.Move(tempFile, FilePath);
            }
        }

        /// <summary>
        /// Clear the whole storage file.
        /// </summary>
        public async Task ClearDataAsync()
        {
            lock (WriteBuilder)
            {
                WriteBuilder.Clear();
            }

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    lock (_fileLock)
                    {
                        File.WriteAllText(FilePath, string.Empty);
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
        /// Get all rows as enumerable, reading one line at a time.
        /// </summary>
        public IEnumerable<T> GetEnumerable()
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
                        yield return DeserializeRow(row);
                    }
                }
            }
        }

        private async Task QueueWrite(string row)
        {
            // Store text in memory
            lock (WriteBuilder)
            {
                WriteBuilder.AppendLine(row);

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
            lock (WriteBuilder)
            {
                var newRows = WriteBuilder.ToString();
                try
                {
                    lock (_fileLock)
                    {
                        File.AppendAllText(FilePath, newRows);
                    }
                    WriteBuilder.Clear();
                }
                catch (Exception)
                {
                    // Retry on next write. Very low chance of exception being thrown here.
                }
                IsWriteQueued = false;
            }
        }

        private T DeserializeRow(string row) => Deserializer(Decode(row));
        private string SerializeItem(T item) => Encode(Serializer(item));

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
