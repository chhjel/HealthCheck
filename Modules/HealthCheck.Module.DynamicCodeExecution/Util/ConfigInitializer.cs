using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Config;
using HealthCheck.Core.Enums;
using System;
using HealthCheck.Core.Util;
#if NETFULL
using HealthCheck.Module.DynamicCodeExecution.Storage;
#endif

namespace HealthCheck.Module.DynamicCodeExecution.Util
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
                new FlatFileDynamicCodeScriptStorage(createPath(@"DCE_Scripts.json"))
            };
            HCExtModuleInitializerUtil.TryExternalLazyFactoryInit(HCModuleType.Code, factory);
#endif
        }
    }
}
