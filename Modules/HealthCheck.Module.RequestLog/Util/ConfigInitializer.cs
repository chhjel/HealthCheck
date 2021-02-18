using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Config;
using HealthCheck.Core.Enums;
using System;
using HealthCheck.Core.Util;
using HealthCheck.RequestLog.Abstractions;
using System.Collections.Generic;
#if NETFULL
using HealthCheck.RequestLog.Services;
#endif

namespace HealthCheck.Module.RequestLog.Util
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
            static Dictionary<Type, IEnumerable<object>> factory(Func<string, string> createPath) => new Dictionary<Type, IEnumerable<object>>
            {
                { typeof(IRequestLogStorage), new object[] {
                    new FlatFileRequestLogStorage(createPath(@"RequestLog_History.json")) } }
            };
            HCExtModuleInitializerUtil.TryExternalLazyFactoryInit(HCModuleType.RequestLog, factory);
#endif
        }
    }
}
