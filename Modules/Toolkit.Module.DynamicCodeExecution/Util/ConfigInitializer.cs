using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Enums;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.DynamicCodeExecution.Abstractions;
using System;
using System.Collections.Generic;
#if NETFULL
using QoDL.Toolkit.Module.DynamicCodeExecution.Storage;
#endif

namespace QoDL.Toolkit.Module.DynamicCodeExecution.Util;

internal class ConfigInitializer : ITKExtModuleInitializer
{
    private static bool _initialized = false;
    private static void SetInitialized() => _initialized = true;

    /// <summary>
    /// Invoked from <see cref="TKGlobalConfig"/> constructor.
    /// </summary>
    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }
        SetInitialized();

        InitLazyFactory();
    }

    private static void InitLazyFactory()
    {
#if NETFULL
        static Dictionary<Type, IEnumerable<object>> factory(Func<string, string> createPath) => new()
        {
            { typeof(IDynamicCodeScriptStorage), new object[] { new FlatFileDynamicCodeScriptStorage(createPath(@"DCE_Scripts.json")) } }
        };
        TKExtModuleInitializerUtil.TryExternalLazyFactoryInit(TKModuleType.Code, factory);
#endif
        /* Otherwise to nothing */
    }
}
