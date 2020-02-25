using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// An object that can be used instead of creating a custom type for dataflow streams.
    /// </summary>
    public class GenericDataflowStreamObject : Dictionary<string, object>, IDataflowEntryWithInsertionTime
    {
        /// <summary>
        /// Time of insertion.
        /// </summary>
        public DateTime? InsertionTime { 
            get => this[nameof(InsertionTime)] as DateTime?;
            set => this[nameof(InsertionTime)] = value;
        }

        /// <summary>
        /// Get value of a field/property name that was stored.
        /// </summary>
        public T Get<T>(string memberName, T defaultValue = default(T))
        {
            if (!this.ContainsKey(memberName))
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

            var type = obj.GetType();
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

            excludedMemberNames = excludedMemberNames ?? Enumerable.Empty<string>();
            memberNames = memberNames.Where(x => !excludedMemberNames.Contains(x)).ToList();

            foreach (var memberName in memberNames)
            {
                try
                {
                    var value = streamObj.GetPropValue(type, obj, memberName);
                    streamObj.Add(memberName, value);
                }
                catch (Exception) { }
            }

            return streamObj;
        }

        private static readonly BindingFlags MemberBindingFlags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private object GetPropValue(Type type, object obj, string memberName)
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
