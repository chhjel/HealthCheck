using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Enums;
using System;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using System.Collections.Generic;
using QoDL.Toolkit.Module.EndpointControl.Storage;

namespace QoDL.Toolkit.Module.EndpointControl.Utils;

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
        static Dictionary<Type, IEnumerable<object>> factory(Func<string, string> createPath) => new()
        {
            { typeof(IEndpointControlRequestHistoryStorage), new object[] {
                new FlatFileEndpointControlRequestHistoryStorage(createPath(@"EndpointControl_History.json")) { PrettyFormat = true }} },
            { typeof(IEndpointControlEndpointDefinitionStorage), new object[] {
                new FlatFileEndpointControlEndpointDefinitionStorage(createPath(@"EndpointControl_Definitions.json")) } },
            { typeof(IEndpointControlRuleStorage), new object[] {
                new FlatFileEndpointControlRuleStorage(createPath(@"EndpointControl_Rules.json")) } }
        };
        TKExtModuleInitializerUtil.TryExternalLazyFactoryInit(TKModuleType.EndpointControl, factory);
    }
}
