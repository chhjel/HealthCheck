using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static QoDL.Toolkit.Core.Util.TKIoCUtils;

namespace QoDL.Toolkit.Core.Config;

/// <summary>
/// Global static config used by Toolkit code.
/// </summary>
public static class TKGlobalConfig
{
    /// <summary>
    /// Determines how new instances of types are created in most Toolkit related code.
    /// <para>For .NET Framework this defaults to <c>DependencyResolver.Current.GetService</c> if the WebUI nuget is used.</para>
		/// <para>Fallback is <c>Activator.CreateInstance</c></para>
    /// </summary>
    public static InstanceResolverDelegate DefaultInstanceResolver { get; set; }

    /// <summary>
    /// Factory that creates information about the current request.
    /// </summary>
    public static Func<TKRequestContext> RequestContextFactory { get; set; }

    /// <summary>
    /// Types ignored in default test-result serialization.
    /// <para>Defaults to actions, and expressions.</para>
    /// </summary>
    public static List<Type> TypesIgnoredInSerialization { get; set; } = new List<Type>
    {
        typeof(Action), typeof(Expression)
    };

    /// <summary>
    /// Generic types ignored in default test-result serialization.
    /// <para>Defaults to actions, funcs, expressions.</para>
    /// </summary>
    public static List<Type> GenericTypesIgnoredInSerialization { get; set; } = new List<Type>
    {
        typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>), typeof(Action<,,,,>),
        typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>), typeof(Func<,,,,,>),
        typeof(Expression<>)
    };

    /// <summary>
    /// Types + all descendants ignored in default test-result serialization.
    /// <para>Defaults expressions and delegates.</para>
    /// </summary>
    public static List<Type> TypesWithDescendantsIgnoredInSerialization { get; set; } = new List<Type>
    {
        typeof(Expression), typeof(Delegate),
    };

    /// <summary>
    /// Namespace-prefixes ignored in default test-result serialization.
    /// </summary>
    public static List<string> NamespacePrefixesIgnoredInSerialization { get; set; }

    /// <summary>
    /// Override logic to find current request IP.
    /// <para>Value is used if not null or whitespace.</para>
    /// </summary>
    public static Func<string> CurrentIPAddressResolver { get; set; }

    /// <summary>
    /// Gets the value of <see cref="DefaultInstanceResolver"/> or attempts <see cref="Activator.CreateInstance(Type)"/> if null.
    /// </summary>
    public static InstanceResolverDelegate GetDefaultInstanceResolver() => DefaultInstanceResolver ?? FallbackInstanceResolver;

    /// <summary>
    /// Signature for instance resolver.
    /// </summary>
    public delegate object InstanceResolverDelegate(Type type, ScopeContainer scope = null);

    /// <summary>
    /// Retrieves the current session id if any.
    /// </summary>
    public static Func<string> GetCurrentSessionId { get; set; }

    /// <summary>
    /// Called whenever util methods that ignores exceptions catches an exception.
    /// </summary>
    public static OnException OnExceptionEvent;

    /// <summary>
    /// Signature for <see cref="OnExceptionEvent"/>.
    /// </summary>
    /// <param name="source">Type that threw the exception.</param>
    /// <param name="method">Name of method that threw the exception</param>
    /// <param name="exception">Exception being thrown.</param>
    public delegate void OnException(Type source, string method, Exception exception);

    /// <summary>
    /// Called whenever some methods that needed extra debugging ignores exceptions catches an exception.
    /// </summary>
    public static OnDebugException OnDebugExceptionEvent;

    /// <summary>
    /// Signature for <see cref="OnDebugException"/>.
    /// </summary>
    /// <param name="source">Type that threw the exception.</param>
    /// <param name="method">Name of method that threw the exception</param>
    /// <param name="exception">Exception being thrown.</param>
    public delegate void OnDebugException(Type source, string method, Exception exception);

    internal static IJsonSerializer Serializer { get; set; }

    internal static T GetService<T>()
        where T: class
        => GetDefaultInstanceResolver()(typeof(T)) as T;

    static TKGlobalConfig()
    {
        // Invoke ITKExtModuleInitializer where available
        var typeNames = new []
        {
            "QoDL.Toolkit.WebUI.Config.ConfigInitializer, QoDL.Toolkit.WebUI",
            "QoDL.Toolkit.WebUI.Assets.ConfigInitializer, QoDL.Toolkit.WebUI.Assets",
            "QoDL.Toolkit.Module.EndpointControl.Utils.ConfigInitializer, QoDL.Toolkit.Module.EndpointControl",
            "QoDL.Toolkit.Module.DynamicCodeExecution.Util.ConfigInitializer, QoDL.Toolkit.Module.DynamicCodeExecution",
            "QoDL.Toolkit.Module.RequestLog.Util.ConfigInitializer, QoDL.Toolkit.Module.RequestLog"
        };

        foreach(var typeName in typeNames)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                var instance = Activator.CreateInstance(type) as ITKExtModuleInitializer;
                instance?.Initialize();
            }
        }
    }

    /// <summary>
    /// Ensure some parts are initialized.
    /// </summary>
    public static void EnsureInitialized()
        => _dummy = !_dummy; // Dummy method to be sure static ctor is invoked.
    private static bool _dummy = false;

    /// <summary>
    /// True if the type is allowed to be instantated.
    /// </summary>
    public static bool AllowActivatingType(Type type)
    {
        return !type.IsInterface
            && !type.IsAbstract
            && !type.IsGenericTypeDefinition
            && !type.IsGenericParameter;
    }

    /// <summary>
    /// Checks the rules from the other properties.
    /// </summary>
    public static bool AllowSerializingType(Type type)
    {
        if (type.IsGenericParameter)
        {
            return false;
        }
        else if (TypesIgnoredInSerialization?.Any(x => type == x) == true)
        {
            return false;
        }
        else if (TypesWithDescendantsIgnoredInSerialization?.Any(x => x.IsAssignableFrom(type)) == true)
        {
            return false;
        }
        else if (NamespacePrefixesIgnoredInSerialization?.Any(x => type?.Namespace?.ToUpper()?.StartsWith(x?.ToUpper()) == true) == true)
        {
            return false;
        }
        else if (type.IsGenericType)
        {
            var baseGenericType = type.GetGenericTypeDefinition();
            if (GenericTypesIgnoredInSerialization?.Any(x => baseGenericType == x) == true)
            {
                return false;
            }
        }
        return true;
    }

    private static object FallbackInstanceResolver(Type type, object scope = null)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch (Exception) {
            return null;
        }
    }
}
