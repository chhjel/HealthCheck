using HealthCheck.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Simple data storage to flatfile.
    /// <para>Requires your own item serialization/deserialization logic.</para>
    /// </summary>
    public class HCSimpleDataStoreWithId<TItem, TId> : HCSimpleDataStore<TItem>, IDataStoreWithEntryId<TItem>
    {
        private Func<TItem, TId> IdSelector { get; set; }
        private Action<TItem, TId> IdSetter { get; set; }
        private Func<IEnumerable<TItem>, TItem, TId> NextIdFactory { get; set; }

        /// <summary>
        /// Simple data storage to flatfile.
        /// <para>Requires your own item serialization/deserialization logic.</para>
        /// </summary>
        /// <param name="filepath">Path to the file where data will be stored.</param>
        /// <param name="serializer">Serialize the wanted properties into string columns.</param>
        /// <param name="deserializer">Deserialize the serialized columns back into their properties.</param>
        /// <param name="idSelector">Selector for the id property.</param>
        /// <param name="idSetter">Sets the item id to the given id.</param>
        /// <param name="nextIdFactory">Creates the next available id.</param>
        public HCSimpleDataStoreWithId(string filepath,
            Func<TItem, string[]> serializer,
            Func<string[], TItem> deserializer,
            Func<TItem, TId> idSelector,
            Action<TItem, TId> idSetter,
            Func<IEnumerable<TItem>, TItem, TId> nextIdFactory)
            : base(filepath, serializer, deserializer)
        {
            IdSelector = idSelector;
            IdSetter = idSetter;
            NextIdFactory = nextIdFactory;
        }

        /// <summary>
        /// Simple data storage to flatfile.
        /// <para>Requires your own item serialization/deserialization logic.</para>
        /// </summary>
        /// <param name="filepath">Path to the file where data will be stored.</param>
        /// <param name="serializer">Serialize the wanted properties into a string.</param>
        /// <param name="deserializer">Deserialize the serialized string back into a object.</param>
        /// <param name="idSelector">Selector for the id property.</param>
        /// <param name="idSetter">Sets the item id to the given id.</param>
        /// <param name="nextIdFactory">Creates the next available id.</param>
        public HCSimpleDataStoreWithId(string filepath,
            Func<TItem, string> serializer,
            Func<string, TItem> deserializer,
            Func<TItem, TId> idSelector,
            Action<TItem, TId> idSetter,
            Func<IEnumerable<TItem>, TItem, TId> nextIdFactory)
            : this(
                 filepath,
                 serializer: new Func<TItem, string[]>(item => new string[] { serializer(item) }),
                 deserializer: new Func<string[], TItem>(columns => columns.Length == 0 ? default : deserializer(columns[0])),
                 idSelector: idSelector,
                 idSetter: idSetter,
                 nextIdFactory: nextIdFactory
        )
        { }

        /// <summary>
        /// Deconstructor. Stores any buffered data before self destructing.
        /// </summary>
        ~HCSimpleDataStoreWithId()
        {
            WriteBufferToFile();
        }

        /// <summary>
        /// Get the FirstOrDefault item with the given id.
        /// </summary>
        public TItem GetItem(TId id)
        {
            return GetEnumerable().FirstOrDefault(x => ItemHasId(x, id));
        }

        /// <summary>
        /// Deletes the item with the given id.
        /// </summary>
        public void DeleteItem(TId id)
            => DeleteWhere(x => ItemHasId(x, id));

        /// <summary>
        /// Add a new row in the file with the given object. Id will be auto-incremented.
        /// </summary>
        public override TItem InsertItem(TItem item)
        {
            var nextId = GetNextIdFor(item);
            SetItemId(item, nextId);
            return base.InsertItem(item);
        }

        /// <summary>
        /// Add the item and autoincrements id if it does not already exist, or updates existing items with the same id as the given item if it does.
        /// <para>Id will be auto-incremented.</para>
        /// </summary>
        public TItem InsertOrUpdateItem(TItem item, Func<TItem, TItem> update = null)
        {
            var id = GetItemId(item);
            var updateCount = UpdateWhere(x => ItemHasId(x, id), update ?? ((old) => item));
            if (updateCount > 0)
            {
                return item;
            }
            else
            {
                return InsertItem(item);
            }
        }

        /// <summary>
        /// Add the item and autoincrements id if it does not already exist, or updates existing items with the same id as the given item if it does.
        /// <para>Id will be auto-incremented.</para>
        /// </summary>
        public void InsertOrUpdateItems(IEnumerable<TItem> items)
        {
            var existingIds = new HashSet<TId>(GetEnumerable().Select(x => GetItemId(x)));
            var itemsDict = items.ToDictionary(x => GetItemId(x), x => x);
            var itemsToInsert = itemsDict.Where(x => !existingIds.Contains(x.Key)).ToArray();
            var itemsToUpdate = itemsDict.Where(x => existingIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            foreach (var item in itemsToInsert)
            {
                InsertItem(item.Value);
            }

            string[] mustContainAny = null;
            if (itemsToUpdate.Count < 1000
                && (typeof(TId) == typeof(string)
                || typeof(TId) == typeof(Guid)
                || typeof(TId) == typeof(int)))
            {
                mustContainAny = itemsToUpdate.Select(x => x.Key.ToString()).ToArray();
            }

            UpdateWhereInternal(
                x => itemsToUpdate.ContainsKey(GetItemId(x)),
                (old) => itemsToUpdate.TryGetValue(GetItemId(old), out var match) ? match : default,
                mustContainAny
            );
        }

        internal TId GetNextIdFor(TItem item)
            => NextIdFactory(GetEnumerable(), item);

        internal TId GetItemId(TItem item)
            => IdSelector(item);

        internal void SetItemId(TItem item, TId id)
            => IdSetter(item, id);

        internal bool ItemHasId(TItem item, TId id)
            => IdsMatch(GetItemId(item), id);

        internal bool IdsMatch(TId a, TId b)
            => Comparer<TId>.Default.Compare(a, b) == 0;
    }
}
