using HealthCheck.Core.Modules.Dataflow.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace HealthCheck.Core.Modules.Dataflow.Models
{
    /// <summary>
    /// An object that can be used instead of creating a custom type for dataflow streams.
    /// </summary>
    public class GenericDataflowStreamObject : Dictionary<string, object>,
        IDataflowEntryWithInsertionTime
    {
        /// <summary>
        /// Time of insertion.
        /// </summary>
        public DateTimeOffset? InsertionTime { 
            get => this[nameof(InsertionTime)] as DateTimeOffset? ?? this[nameof(InsertionTime)] as DateTime?;
            set => this[nameof(InsertionTime)] = value;
        }

        internal List<Func<IEnumerable<GenericDataflowStreamObject>, DataflowStreamFilter, IEnumerable<GenericDataflowStreamObject>>>
            PropertyFilters = new List<Func<IEnumerable<GenericDataflowStreamObject>, DataflowStreamFilter, IEnumerable<GenericDataflowStreamObject>>>();

        /// <summary>
        /// Get value of a field/property name that was stored.
        /// </summary>
        public T Get<T>(string memberName, T defaultValue = default)
        {
            if (!ContainsKey(memberName))
            {
                return defaultValue;
            }

            try
            {
                return (T)this[memberName];
            }
            catch(Exception) { return defaultValue; }
        }

        /// <summary>
        /// Creates an object that can be used instead of creating a custom type for dataflow streams.
        /// </summary>
        /// <param name="obj">Object to read property/field values from.</param>
        /// <param name="memberNames">Name of properties and fields to include. If left null all public/private instance values will be used.</param>
        /// <param name="excludedMemberNames">List of property/field names to exclude.</param>
        public static GenericDataflowStreamObject Create<T>(
            T obj,
            IEnumerable<string> memberNames = null,
            IEnumerable<string> excludedMemberNames = null)
        {
            var streamObj = new GenericDataflowStreamObject();
            if (obj == null)
            {
                return streamObj;
            }

            var type = typeof(T);
            memberNames = GatherMemberNames(memberNames, excludedMemberNames, type);

            foreach (var memberName in memberNames)
            {
                try
                {
                    var value = GetPropValue(type, obj, memberName);
                    streamObj.Add(memberName, value);
                }
                catch (Exception) { }
            }

            return streamObj;
        }

        private static IEnumerable<string> GatherMemberNames(IEnumerable<string> memberNames, IEnumerable<string> excludedMemberNames, Type type)
        {
            if (memberNames == null)
            {
                memberNames =
                    type.GetProperties(MemberBindingFlags)
                        .Where(x => !x.IsSpecialName)
                        .Select(x => x.Name)
                    .Union(type.GetFields(MemberBindingFlags)
                        .Where(x => !x.IsSpecialName && x.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                        .Select(x => x.Name))
                    .ToList();
            }

            excludedMemberNames ??= Enumerable.Empty<string>();
            memberNames = memberNames.Where(x => !excludedMemberNames.Contains(x)).ToList();

            return memberNames;
        }

        internal class AutoFilter<TEntry>
        {
            public string MemberName { get; set; }
            public Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>> Filter { get; set; }
        }
        internal static List<AutoFilter<TEntry>>
            CreateAutoFilters<TEntry>(
                IEnumerable<string> memberNames = null,
                IEnumerable<string> excludedMemberNames = null)
        {
            var itemType = typeof(TEntry);
            memberNames = GatherMemberNames(memberNames, excludedMemberNames, itemType);

            var filters = new List<AutoFilter<TEntry>>();
            foreach (var memberName in memberNames)
            {
                try
                {
                    Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>> filter = null;

                    if (typeof(TEntry) == typeof(GenericDataflowStreamObject))
                    {
                        filter = (Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>>)
                            new Func<IEnumerable<GenericDataflowStreamObject>, DataflowStreamFilter, IEnumerable<GenericDataflowStreamObject>>(
                            (items, filter) => filter.FilterContains(items, memberName, x => x.Get<object>(memberName)?.ToString()));
                    }
                    else
                    {
                        Func<TEntry, object> valueGetter = null;
                        Type memberType = null;
                        var prop = typeof(TEntry).GetProperty(memberName);
                        if (prop != null)
                        {
                            memberType = prop.PropertyType;
                            valueGetter = (e) => prop.GetValue(e) as object;
                        }
                        if (valueGetter == null)
                        {
                            var field = typeof(TEntry).GetField(memberName);
                            if (field != null)
                            {
                                memberType = field.FieldType;
                                valueGetter = (e) => field.GetValue(e) as object;
                            }
                        }

                        var stringifiedGetter = CreateValueGetter(valueGetter, memberType);

                        if (stringifiedGetter != null)
                        {
                            filter = new Func<IEnumerable<TEntry>, DataflowStreamFilter, IEnumerable<TEntry>>(
                                (items, filter) =>
                                {
                                    try
                                    {
                                        return filter.FilterContains(items, memberName, x => stringifiedGetter(x));
                                    }
                                    catch (Exception) { return items; }
                                });
                        }
                    }

                    if (filter != null)
                    {
                        filters.Add(new AutoFilter<TEntry>
                        {
                            MemberName = memberName,
                            Filter = filter
                        });
                    }
                }
                catch (Exception) { }
            }
            return filters;
        }

        private static Func<TEntry, string> CreateValueGetter<TEntry>(Func<TEntry, object> objectGetter, Type memberType)
        {
            if (objectGetter == null || memberType == null)
            {
                return null;
            }
            // Lists && arrays
            else if (memberType.IsGenericType 
                && memberType.GenericTypeArguments.Length == 1
                && memberType.GetInterfaces().Contains(typeof(System.Collections.IList)))
            {
                return (entry) =>
                {
                    var memberValue = objectGetter?.Invoke(entry);
                    if (!(memberValue is System.Collections.IList list))
                    {
                        return "";
                    }

                    var builder = new StringBuilder();
                    foreach(var item in list)
                    {
                        builder.AppendLine(item?.ToString());
                    }
                    return builder.ToString();
                };
            }
            // Dictionaries
            else if (memberType.IsGenericType
                && memberType.GenericTypeArguments.Length == 2
                && memberType.GetInterfaces().Contains(typeof(System.Collections.IDictionary)))
            {
                return (entry) =>
                {
                    var memberValue = objectGetter?.Invoke(entry);
                    if (!(memberValue is System.Collections.IDictionary dictionary))
                    {
                        return "";
                    }

                    var builder = new StringBuilder();
                    foreach (var key in dictionary.Keys)
                    {
                        builder.AppendLine($"{key}:{dictionary[key]}");
                    }
                    return builder.ToString();
                };
            }

            // Fallback to stringified member value
            return (entry) => objectGetter?.Invoke(entry)?.ToString();
        }

        private static readonly BindingFlags MemberBindingFlags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private static object GetPropValue(Type type, object obj, string memberName)
        {

            if (type.GetProperty(memberName, MemberBindingFlags) != null)
            {
                return type.GetProperty(memberName, MemberBindingFlags).GetValue(obj);
            }
            else if (type.GetField(memberName, MemberBindingFlags) != null)
            {
                return type.GetField(memberName, MemberBindingFlags).GetValue(obj);
            }

            throw new ArgumentException($"No property/field with the name '{memberName}' was found.");
        }
    }
}
