using QoDL.Toolkit.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QoDL.Toolkit.Core.Util;

/// <summary>Utils to simplify life from Toolkit tests and DCE.</summary>
public static class TKReflectionUtils
{
    /// <summary>
    /// Attempt to invoke a method on the given type.
    /// <para>An instance will be attempted created.</para>
    /// </summary>
    /// <typeparam name="TClass">Type of object to invoke method on.</typeparam>
    /// <param name="methodName">Name of the method to invoke</param>
    /// <param name="parameters">Method parameters if any</param>
    /// <returns>Method return value, or null if void.</returns>
    public static object TryInvokeMethod<TClass>(string methodName, params object[] parameters)
        where TClass : class
        => TryInvokeMethodExt<TClass>(methodName, parameters);

    /// <summary>
    /// Attempt to invoke a method on the given type.
    /// <para>An instance will be attempted created.</para>
    /// </summary>
    /// <typeparam name="TClass">Type of object to invoke method on.</typeparam>
    /// <typeparam name="TResult">Type the result will be cast to.</typeparam>
    /// <param name="methodName">Name of the method to invoke</param>
    /// <param name="parameters">Method parameters if any</param>
    /// <returns>Method return value, or null if void.</returns>
    public static TResult TryInvokeMethod<TClass, TResult>(string methodName, params object[] parameters)
        where TClass : class
        => (TResult)TryInvokeMethodExt<TClass>(methodName, parameters);

    /// <summary>
    /// Attempt to invoke a method on the given type.
    /// <para>An instance will be attempted created.</para>
    /// </summary>
    /// <typeparam name="TClass">Type of object to invoke method on.</typeparam>
    /// <param name="methodName">Name of the method to invoke</param>
    /// <param name="parameters">Method parameters if any</param>
    /// <param name="genericParameters">Generic method parameters if any</param>
    /// <returns>Method return value, or null if void.</returns>
    public static object TryInvokeMethodExt<TClass>(string methodName,
        object[] parameters = null, Type[] genericParameters = null)
        where TClass : class
    {
        var instance = TKIoCUtils.GetInstanceExt<TClass>();
        return TryInvokeMethodExt(instance?.GetType() ?? typeof(TClass), instance, methodName, parameters, genericParameters);
    }

    /// <summary>
    /// Attempt to invoke a method on the given type.
    /// <para>An instance will be attempted created.</para>
    /// </summary>
    /// <typeparam name="TClass">Type of object to invoke method on.</typeparam>
    /// <typeparam name="TReturn">Type of the method return value.</typeparam>
    /// <param name="methodName">Name of the method to invoke</param>
    /// <param name="parameters">Method parameters if any</param>
    /// <param name="genericParameters">Generic method parameters if any</param>
    /// <returns>Method return value, or null if void.</returns>
    public static TReturn TryInvokeMethodExt<TClass, TReturn>(string methodName,
        object[] parameters = null, Type[] genericParameters = null)
        where TClass : class
    {
        var instance = TKIoCUtils.GetInstanceExt<TClass>();
        return (TReturn)TryInvokeMethodExt(instance?.GetType() ?? typeof(TClass), instance, methodName, parameters, genericParameters);
    }

    /// <summary>
    /// Attempt to invoke a method on the given type.
    /// </summary>
    /// <param name="instance">Object instance to invoke method on</param>
    /// <param name="methodName">Name of the method to invoke</param>
    /// <param name="parameters">Method parameters if any</param>
    /// <param name="genericParameters">Generic method parameters if any</param>
    /// <returns>Method return value, or null if void.</returns>
    public static object TryInvokeMethodExt(object instance, string methodName,
        object[] parameters = null, Type[] genericParameters = null)
        => TryInvokeMethodExt(instance.GetType(), instance, methodName, parameters, genericParameters);

    private static object TryInvokeMethodExt(Type type, object instance, string methodName,
        object[] parameters = null, Type[] genericParameters = null)
    {
        var methods = type.GetMethods(
            BindingFlags.NonPublic
            | BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.FlattenHierarchy);
        parameters ??= new object[0];
        genericParameters ??= new Type[0];

        var method = methods.FirstOrDefault(x =>
            x.Name == methodName
            && x.GetParameters().Length == parameters.Length
            && (
                (!x.ContainsGenericParameters && genericParameters.Length == 0)
                ||
                (x.ContainsGenericParameters && x.GetGenericArguments().Length == genericParameters.Length)
            )
        );

        if (method?.IsGenericMethod == true)
        {
            method = method.MakeGenericMethod(genericParameters);
        }
        return method?.Invoke(instance, parameters);
    }

    /// <summary>
    /// Attempt to get the value of a member.
    /// <para>An instance will be attempted created.</para>
    /// </summary>
    /// <typeparam name="TClass">Type of object to invoke method on.</typeparam>
    /// <param name="memberName">Name of the member to get the value from.</param>
    /// <returns>Value of the member.</returns>
    public static object TryGetMemberValue<TClass>(string memberName)
        where TClass : class
    {
        var instance = TKIoCUtils.GetInstanceExt<TClass>();
        return TryGetMemberValue(instance?.GetType() ?? typeof(TClass), instance, memberName);
    }

    /// <summary>
    /// Attempt to get the value of a member.
    /// </summary>
    /// <param name="instance">Object instance to get the value from.</param>
    /// <param name="memberName">Name of the member to get the value from.</param>
    /// <returns>Value of the member.</returns>
    public static object TryGetMemberValue(object instance, string memberName)
        => TryGetMemberValue(instance.GetType(), instance, memberName);

    /// <summary>
    /// Attempts to create a new instance of the given type, ignoring any errors and returning null on failure.
    /// </summary>
    public static object TryActivate(Type type)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Name and type of a member.
    /// </summary>
    public class TypeMemberData
    {
        /// <summary>
        /// Name of the member or its path.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the member.
        /// </summary>
        public Type Type { get; set; }

        /// <summary></summary>
        public override string ToString() => $"{Name} [{Type.Name}]";
    }

    /// <summary>
    /// Get a list of members recursively.
    /// <para><see cref="TypeMemberData.Name" /> will be the dotted path to the member.</para>
    /// <para>Ignores members that can't be read and also indexers.</para>
    /// </summary>
    public static List<TypeMemberData> GetTypeMembersRecursive(Type type, int maxDepth, TKMemberFilterRecursive filter = null)
        => GetTypeMembersRecursive(type, null, 0, maxDepth, null, filter)
                .GroupBy(x => x.Name).Select(x => x.First())
                .ToList();

    private static List<TypeMemberData> GetTypeMembersRecursive(Type type, string path = null, int currentDepth = 0, int maxDepth = 4,
        List<TypeMemberData> worklist = null, TKMemberFilterRecursive filter = null)
    {
        if (type == null) return new();

        var paths = worklist ?? new List<TypeMemberData>();
        if (currentDepth >= maxDepth) return paths;

        static bool allowRecurseType(Type type)
        {
            var genericTypeDef = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
            return !type.IsSpecialName
                && !type.IsValueType
                && !type.IsPrimitive
                && !type.IsArray
                && type.Namespace?.StartsWith("System.") != true
                && type.Namespace != "System"
                && type.Module.ScopeName != "CommonLanguageRuntimeLibrary";
        }

        var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
        var props = type.GetProperties(bindingFlags);
        foreach (var prop in props)
        {
            var name = $"{(path == null ? "" : $"{path}.")}{prop.Name}";
            if (filter?.AllowMember(prop, name) == false)
            {
                continue;
            }

            paths.Add(new TypeMemberData
            {
                Name = name,
                Type = prop.PropertyType
            });

            string indexedSuffix = null;
            var typeToRecurse = prop.PropertyType;
            if (typeToRecurse.SupportsGetCollectionValueIndexed())
            {
                indexedSuffix = "[0]";
                typeToRecurse = typeToRecurse.GetUnderlyingCollectionType() ?? typeToRecurse;
                paths.Add(new TypeMemberData
                {
                    Name = name + indexedSuffix,
                    Type = prop.PropertyType.GetUnderlyingCollectionType() ?? typeof(object)
                });
            }
            if (!prop.IsSpecialName
                && prop.GetMethod != null
                && prop.CanRead
                && allowRecurseType(typeToRecurse)
                && prop.GetCustomAttribute<CompilerGeneratedAttribute>() == null
                && prop.GetIndexParameters()?.Any() != true)
            {
                GetTypeMembersRecursive(typeToRecurse,
                    $"{(string.IsNullOrWhiteSpace(path) ? "" : $"{path}.")}{prop.Name}{indexedSuffix}",
                    currentDepth + 1, maxDepth, paths, filter);
            }
        }

        var fields = type.GetFields();
        foreach (var field in fields)
        {
            var name = $"{(path == null ? "" : $"{path}.")}{field.Name}";
            if (filter?.AllowMember(field, name) == false)
            {
                continue;
            }

            paths.Add(new TypeMemberData
            {
                Name = name,
                Type = field.FieldType
            });

            string indexedSuffix = null;
            var typeToRecurse = field.FieldType;
            if (typeToRecurse.SupportsGetCollectionValueIndexed())
            {
                indexedSuffix = "[0]";
                typeToRecurse = typeToRecurse.GetUnderlyingCollectionType() ?? typeToRecurse;
                paths.Add(new TypeMemberData
                {
                    Name = name + indexedSuffix,
                    Type = field.FieldType.GetUnderlyingCollectionType() ?? typeof(object)
                });
            }
            if (!field.IsSpecialName
                && allowRecurseType(typeToRecurse)
                && field.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
            {
                GetTypeMembersRecursive(typeToRecurse,
                    $"{(string.IsNullOrWhiteSpace(path) ? "" : $"{path}.")}{field.Name}{indexedSuffix}",
                    currentDepth + 1, maxDepth, paths, filter);
            }
        }

        return paths;
    }

    /// <summary>
    /// Get a property by its path including its underlying type/property type. Supports indexers.
    /// </summary>
    public static (PropertyInfo, Type) GetProperty(Type type, string membName)
    {
        if (type == null) return (null, null);

        var cleanName = membName;
        if (cleanName.Contains("["))
        {
            cleanName = cleanName.Substring(0, cleanName.IndexOf("["));
        }

        // First look for matching property
        var prop = GetLowestProperty(type, cleanName);
        if (prop == null) return (null, null);
        else if (prop.CanRead && !prop.PropertyType.SupportsGetCollectionValueIndexed())
        {
            return (prop, prop.PropertyType);
        }

        var propType = prop.PropertyType;

        // Check if member is indexed
        var hasIndexer = membName.Contains("[") && membName.Contains("]");
        //int? targetIndex = null;
        //if (membName.Contains("["))
        //{
        //	var strIndex = membName.Substring(membName.IndexOf("[") + 1);
        //	strIndex = strIndex.Substring(0, strIndex.IndexOf("]"));
        //	targetIndex = int.Parse(strIndex);
        //}
        if (hasIndexer && /*targetIndex != null &&*/ propType?.SupportsGetCollectionValueIndexed() == true)
        {
            return (prop, propType.GetUnderlyingCollectionType());
        }

        return (prop, prop.PropertyType);
    }

    /// <summary>
    /// Get a property by its dotted and optionally indexed path.
    /// <para>Returns null if not found.</para>
    /// </summary>
    public static object GetValue(object rootInstance, string path)
    {
        var instance = rootInstance;
        if (instance == null) return null;

        object getValue(string membName, out bool found)
        {
            found = false;
            if (instance == null) return null;

            var cleanName = membName;
            if (cleanName.Contains("["))
            {
                cleanName = cleanName.Substring(0, cleanName.IndexOf("["));
            }

            // First look for matching property
            object memberValue = null;
            var prop = GetLowestProperty(instance.GetType(), cleanName);
            if (prop != null && prop.CanRead && prop.GetIndexParameters()?.Any() != true)
            {
                found = true;
                memberValue = prop.GetValue(instance);
            }

            // Then look for matching field
            if (!found)
            {
                var field = GetLowestField(instance.GetType(), cleanName);
                if (field != null)
                {
                    found = true;
                    memberValue = field.GetValue(instance);
                }
            }

            // Check if member is indexed
            int? targetIndex = null;
            if (membName.Contains("["))
            {
                var strIndex = membName.Substring(membName.IndexOf("[") + 1);
                strIndex = strIndex.Substring(0, strIndex.IndexOf("]"));
                targetIndex = int.Parse(strIndex);
            }
            if (targetIndex != null && memberValue?.GetType()?.SupportsGetCollectionValueIndexed() == true)
            {
                return memberValue.GetType().GetCollectionValueIndexed(memberValue, targetIndex.Value);
            }

            return memberValue;
        }

        while (path.Contains("."))
        {
            var memberName = path.Substring(0, path.IndexOf("."));
            path = path.Substring(memberName.Length + 1);

            instance = getValue(memberName, out var wasFound);
            if (!wasFound || instance == null)
            {
                return null;
            }
        }

        return getValue(path, out _);
    }


    /// <summary>
    /// Get the lowest defined property on the given type with the given name.
    /// </summary>
    public static PropertyInfo GetLowestProperty(Type type, string name)
    {
        while (type != null)
        {
            var property = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(x => x.Name == name);
            if (property != null)
            {
                return property;
            }
            type = type.BaseType;
        }
        return null;
    }

    /// <summary>
    /// Get the lowest defined field on the given type with the given name.
    /// </summary>
    public static FieldInfo GetLowestField(Type type, string name)
    {
        while (type != null)
        {
            var field = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(x => x.Name == name);
            if (field != null)
            {
                return field;
            }
            type = type.BaseType;
        }
        return null;
    }

    private static object TryGetMemberValue(Type type, object instance, string memberName)
    {
        var members = type.GetMembers(
            BindingFlags.NonPublic | BindingFlags.Public
            | BindingFlags.Static | BindingFlags.Instance
            | BindingFlags.GetProperty
            | BindingFlags.GetField
            | BindingFlags.FlattenHierarchy);

        var member = members.FirstOrDefault(x => x.Name == memberName);
        return GetMemberValue(member, instance);
    }

    private static object GetMemberValue(this MemberInfo memberInfo, object instance)
    {
        return memberInfo.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)memberInfo).GetValue(instance),
            MemberTypes.Property => ((PropertyInfo)memberInfo).GetValue(instance),
            _ => null,
        };
    }
}
