using HealthCheck.Core.Abstractions;
using System;
using System.Collections.Generic;

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
        /// Types ignored in default test-result serialization.
        /// </summary>
        public static List<Type> TypesIgnoredInSerialization { get; set; }

        /// <summary>
        /// Types + all descendants ignored in default test-result serialization.
        /// </summary>
        public static List<Type> TypesWithDescendantsIgnoredInSerialization { get; set; }
        
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
