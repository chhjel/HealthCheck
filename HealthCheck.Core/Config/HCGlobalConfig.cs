using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HealthCheck.Core.Config
{
    /// <summary>
    /// Global static config used by HealthCheck code.
    /// </summary>
    public static class HCGlobalConfig
    {
        /// <summary>
        /// Determines how new instances of types are created in most HealthCheck related code.
        /// <para>For .NET Framework this defaults to <c>DependencyResolver.Current.GetService</c> if the WebUI nuget is used.</para>
		/// <para>Fallback is <c>Activator.CreateInstance</c></para>
        /// </summary>
        public static Func<Type, object> DefaultInstanceResolver { get; set; }

        /// <summary>
        /// Factory that creates information about the current request.
        /// </summary>
        public static Func<HCRequestContext> RequestContextFactory { get; set; }

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
        public static Func<Type, object> GetDefaultInstanceResolver() => DefaultInstanceResolver ?? FallbackInstanceResolver;

        internal static T GetService<T>()
            where T: class
            => GetDefaultInstanceResolver() as T;

        static HCGlobalConfig()
        {
            // Invoke IHCExtModuleInitializer where available
            var typeNames = new []
            {
                "HealthCheck.WebUI.Config.ConfigInitializer, HealthCheck.WebUI",
                "HealthCheck.Module.EndpointControl.Utils.ConfigInitializer, HealthCheck.Module.EndpointControl",
                "HealthCheck.Module.DynamicCodeExecution.Util.ConfigInitializer, HealthCheck.Module.DynamicCodeExecution",
                "HealthCheck.Module.RequestLog.Util.ConfigInitializer, HealthCheck.Module.RequestLog"
            };

            foreach(var typeName in typeNames)
            {
                var type = Type.GetType(typeName);
                if (type != null)
                {
                    var instance = Activator.CreateInstance(type) as IHCExtModuleInitializer;
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

        private static object FallbackInstanceResolver(Type type)
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
}
