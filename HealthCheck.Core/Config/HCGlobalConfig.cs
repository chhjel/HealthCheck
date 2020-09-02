using System;

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
        /// Gets the value of <see cref="DefaultInstanceResolver"/> or <see cref="Activator.CreateInstance(Type)"/> if null.
        /// </summary>
        public static Func<Type, object> GetDefaultInstanceResolver() => DefaultInstanceResolver ?? Activator.CreateInstance;

        static HCGlobalConfig()
        {
            // Invoke WebUI ConfigInitializer if available. To be made more generic later.
            var webUIInitializerType = Type.GetType("HealthCheck.WebUI.Config.ConfigInitializer, HealthCheck.WebUI");
            if (webUIInitializerType != null)
            {
                var method = webUIInitializerType.GetMethod("Initialize");
                method?.Invoke(null, new object[0]);
            }
        }
    }
}
