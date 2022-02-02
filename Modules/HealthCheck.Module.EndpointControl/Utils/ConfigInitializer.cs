using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Config;
using HealthCheck.Core.Enums;
using System;
using HealthCheck.Core.Util;
using HealthCheck.Module.EndpointControl.Abstractions;
using System.Collections.Generic;
using HealthCheck.Module.EndpointControl.Storage;

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
            static Dictionary<Type, IEnumerable<object>> factory(Func<string, string> createPath) => new()
            {
                { typeof(IEndpointControlRequestHistoryStorage), new object[] {
                    new FlatFileEndpointControlRequestHistoryStorage(createPath(@"EndpointControl_History.json")) { PrettyFormat = true }} },
                { typeof(IEndpointControlEndpointDefinitionStorage), new object[] {
                    new FlatFileEndpointControlEndpointDefinitionStorage(createPath(@"EndpointControl_Definitions.json")) } },
                { typeof(IEndpointControlRuleStorage), new object[] {
                    new FlatFileEndpointControlRuleStorage(createPath(@"EndpointControl_Rules.json")) } }
            };
            HCExtModuleInitializerUtil.TryExternalLazyFactoryInit(HCModuleType.EndpointControl, factory);
        }
    }
}
