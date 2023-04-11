using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace QoDL.Toolkit.Core.Extensions;

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

    // todo: support IEnumerable without index using skip & firstordefault

    /// <summary>
    /// Determines if the type is supported used with <see cref="GetCollectionValueIndexed"/>.
    /// </summary>
    public static bool SupportsGetCollectionValueIndexed(this Type type)
        => type != typeof(string)
        && (
            type.IsListOrArray()
            || typeof(System.Collections.IEnumerable).IsAssignableFrom(type)
            || (typeof(System.Collections.ICollection).IsAssignableFrom(type))
            || type.GetProperties().Any(p => p.GetIndexParameters().Count(x => x.ParameterType == typeof(int)) == 1)
        );

    /// <summary>
    /// Try to get underlying array/list type.
    /// </summary>
    public static Type GetUnderlyingCollectionType(this Type type)
    {
        if (type.IsArray)
            return type.GetElementType();
        else if (type.IsGenericType && typeof(System.Collections.IList).IsAssignableFrom(type))
            return type.GenericTypeArguments[0];
        else if (type.IsGenericType && typeof(System.Collections.ICollection).IsAssignableFrom(type))
            return type.GenericTypeArguments[0];
        else if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            return type.GenericTypeArguments[0];

        while (type != null && type != typeof(object))
        {
            type = type?.BaseType;
            if (type == null) break;
            var underlyingType = GetUnderlyingCollectionType(type);
            if (underlyingType != null) return underlyingType;
        }

        return null;
    }

    /// <summary>
    /// Try to get value from an indexer.
    /// <para>Allows for -1 for last, -2 for 2nd last etc.</para>
    /// </summary>
    public static object GetCollectionValueIndexed(this Type type, object instance, int index)
    {
        int? resolveIndex(int length)
        {
            var idx = index;
            if (idx < 0) idx = length + idx;
            return (idx >= length || idx < 0) ? null : idx;
        }

        // Array
        if (type.IsArray)
        {
            var array = (Array)instance;
            var resolvedIndex = resolveIndex(array.Length);
            return resolvedIndex == null ? null : array.GetValue(resolvedIndex.Value);
        }
        // IList
        else if (instance is System.Collections.IList list)
        {
            var resolvedIndex = resolveIndex(list.Count);
            return resolvedIndex == null ? null : list[resolvedIndex.Value];
        }
        // ICollection<T>
        else if (instance is System.Collections.ICollection collectionT && type.IsGenericType)
        {
            var resolvedIndex = resolveIndex(collectionT.Cast<object>().Count());
            return resolvedIndex == null ? null : collectionT.Cast<object>().Skip(resolvedIndex.Value).FirstOrDefault();
        }
        // ICollection
        else if (instance is System.Collections.ICollection collection)
        {
            var resolvedIndex = resolveIndex(collection.Count);
            return resolvedIndex == null ? null : collection.Cast<object>().Skip(resolvedIndex.Value).FirstOrDefault();
        }
        // IEnumerable<T>
        else if (instance is System.Collections.IEnumerable enumerableT && type.IsGenericType)
        {
            var resolvedIndex = resolveIndex(enumerableT.Cast<object>().Count());
            return resolvedIndex == null ? null : enumerableT.Cast<object>().Skip(resolvedIndex.Value).FirstOrDefault();
        }
        // IEnumerable with positive index
        else if (instance is System.Collections.IEnumerable enumerable && index >= 0)
        {
            var resolvedIndex = resolveIndex(index);
            return resolvedIndex == null ? null : enumerable.Cast<object>().Skip(resolvedIndex.Value).FirstOrDefault();
        }

        var finalResolvedIndex = resolveIndex(index);
        if (finalResolvedIndex == null) return null;

        var indexer = type.GetProperties()
            .FirstOrDefault(p => p.GetIndexParameters().Count(x => x.ParameterType == typeof(int)) == 1);
        if (indexer != null)
        {
            return indexer.GetValue(instance, new object[] { finalResolvedIndex.Value });
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
