using QoDL.Toolkit.Core.Enums;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Internally used between assemblies.
/// </summary>
public static class TKExtModuleInitializerUtil
{
    /// <summary>
    /// Internally used between assemblies.
    /// </summary>
    public static void TryExternalLazyFactoryInit(TKModuleType moduleType,
        Func<Func<string, string>, Dictionary<Type, IEnumerable<object>>> instanceFactory)
    {
        var typeName = "QoDL.Toolkit.WebUI.Services.TKLazyFlatFileFactory, QoDL.Toolkit.WebUI";
        var type = Type.GetType(typeName);
        if (type != null)
        {
            var prop = type.GetProperty("ExternalModuleInstanceFactories");
            if (prop?.GetValue(null) is Dictionary<TKModuleType, Func<Func<string, string>, Dictionary<Type, IEnumerable<object>>>> dict)
            {
                dict[moduleType] = instanceFactory;
            }
        }
    }
}
