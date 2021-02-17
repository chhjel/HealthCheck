using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Config;
using HealthCheck.Core.Enums;
using System;
using HealthCheck.Core.Util;
#if NETFULL
using HealthCheck.Module.EndpointControl.Storage;
#endif

namespace HealthCheck.Module.EndpointControl.Utils
{
    internal class ConfigInitializer : IHCExtModuleInitializer
    {
        private static bool _initialized = false;
        private static void SetInitialized() => _initialized = true;

        /// <summary>
        /// Invoked from <see cref="HCGlobalConfig"/> constructor.
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
            static object[] factory(Func<string, string> createPath) => new object[]
            {
                new FlatFileEndpointControlRequestHistoryStorage(createPath(@"EndpointControl_History.json")) { PrettyFormat = true },
                new FlatFileEndpointControlEndpointDefinitionStorage(createPath(@"EndpointControl_Definitions.json")),
                new FlatFileEndpointControlRuleStorage(createPath(@"EndpointControl_Rules.json"))
            };
            HCExtModuleInitializerUtil.TryExternalLazyFactoryInit(HCModuleType.EndpointControl, factory);
#endif
        }
    }
}
