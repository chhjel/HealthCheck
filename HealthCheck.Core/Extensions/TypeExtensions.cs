using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace HealthCheck.Core.Extensions
{
    /// <summary>
    /// Extensions related to <see cref="Type"/>s.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Checks if the type is anonymous.
        /// </summary>
        public static bool IsAnonymous(this Type type)
        {
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        /// <summary>
        /// Get a prettier name for the type.
        /// </summary>
        public static string GetFriendlyTypeName(this Type type, Dictionary<string, string> aliases = null)
        {
            if (type == null)
            {
                return null;
            }
            else if (IsAnonymous(type))
            {
                return "AnonymousType";
            }

            var friendlyNameBuilder = new StringBuilder();
            friendlyNameBuilder.Append(type.Name);
            if (type.IsGenericType)
            {
                int iBacktick = friendlyNameBuilder.ToString().IndexOf('`');
                if (iBacktick > 0)
                {
                    var friendlyName = friendlyNameBuilder.ToString().Remove(iBacktick);
                    friendlyNameBuilder.Clear();
                    friendlyNameBuilder.Append(friendlyName);
                }
                friendlyNameBuilder.Append("<");
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetFriendlyTypeName(typeParameters[i]);
                    typeParamName = ResolveInputTypeAlias(typeParamName, aliases);
                    friendlyNameBuilder.Append(i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyNameBuilder.Append(">");
            }


            var resolvedName = friendlyNameBuilder.ToString();
            return ResolveInputTypeAlias(resolvedName, aliases);
        }

        /// <summary>
        /// True if the generic type definition is Nullable.
        /// </summary>
        public static bool IsNullable(this Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        /// <summary>
        /// Determines whether an instance of a specified type can be assigned to an instance of the current type.
        /// <para>If the other type is a nullable its underlying type will also be checked.</para>
        /// </summary>
        public static bool IsAssignableFromIncludingNullable(this Type type, Type other)
        {
            if (type.IsAssignableFrom(other)) return true;
            else if (other.IsGenericType
                && other.GetGenericTypeDefinition() == typeof(Nullable<>)
                && type.IsAssignableFrom(Nullable.GetUnderlyingType(other)))
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Determines if the type is a list or array.
        /// </summary>
        public static bool IsListOrArray(this Type type)
            => type.IsArray
            || typeof(System.Collections.IList).IsAssignableFrom(type);

        /// <summary>
        /// Determines if the type contains a numeric index.
        /// </summary>
        public static bool HasNumericIndex(this Type type)
            // todo cache?
            => type.IsListOrArray()
            || type.GetProperties().Any(p => p.GetIndexParameters().Count(x => x.ParameterType == typeof(int)) == 1);

        /// <summary>
        /// Try to get underlying array/list type.
        /// </summary>
        public static Type GetUnderlyingEnumerableType(this Type type)
        {
            if (type.IsArray)
                return type.GetElementType();
            else if (type.IsGenericType && typeof(System.Collections.IList).IsAssignableFrom(type))
                return type.GenericTypeArguments[0];
            else
                return null;
        }

        /// <summary>
        /// Try to get value from an indexer.
        /// </summary>
        public static object GetIndexedValue(this Type type, object instance, int index)
        {
            if (type.IsArray)
            {
                var array = (Array)instance;
                return index >= array.Length ? null : array.GetValue(index);
            }
            else if (instance is System.Collections.IList list)
            {
                return index >= list.Count ? null : list[index];
            }

            var indexer = type.GetProperties()
                .FirstOrDefault(p => p.GetIndexParameters().Count(x => x.ParameterType == typeof(int)) == 1);
            if (indexer != null)
            {
                return indexer.GetValue(instance, new object[] { index });
            }

            return null;
        }

        private static string ResolveInputTypeAlias(string name, Dictionary<string, string> aliases = null)
        {
            if (aliases?.Any() != true)
            {
                return name;
            }
            return aliases.ContainsKey(name) ? aliases[name] : name;
        }
    }
}
