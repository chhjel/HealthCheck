using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Config;

#if NETFRAMEWORK
using System.Web.Mvc;
#endif

namespace HealthCheck.WebUI.Config
{
    internal class ConfigInitializer : IHCExtModuleInitializer
    {
        /// <summary>
        /// Invoked from <see cref="HCGlobalConfig"/> constructor.
        /// </summary>
        public void Initialize()
        {
            if (HCGlobalConfig.DefaultInstanceResolver == null)
            {
                SetDefaultInstanceResolver();
            }
        }

        private static void SetDefaultInstanceResolver()
        {
            /* No effect for other targets atm. */
#if NETFRAMEWORK
            HCGlobalConfig.DefaultInstanceResolver = (type) => DependencyResolver.Current.GetService(type);
#endif
        }
    }
}
