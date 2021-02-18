using HealthCheck.Core.Enums;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Internally used between assemblies.
    /// </summary>
    public static class HCExtModuleInitializerUtil
    {
        /// <summary>
        /// Internally used between assemblies.
        /// </summary>
        public static void TryExternalLazyFactoryInit(HCModuleType moduleType,
            Func<Func<string, string>, Dictionary<Type, IEnumerable<object>>> instanceFactory)
        {
            var typeName = "HealthCheck.WebUI.Services.HCLazyFlatFileFactory, HealthCheck.WebUI";
            var type = Type.GetType(typeName);
            if (type != null)
            {
                var prop = type.GetProperty("ExternalModuleInstanceFactories");
                var dict = prop?.GetValue(null) as Dictionary<HCModuleType, Func<Func<string, string>, Dictionary<Type, IEnumerable<object>>>>;
                if (dict != null)
                {
                    dict[moduleType] = instanceFactory;
                }
            }
        }
    }
}
