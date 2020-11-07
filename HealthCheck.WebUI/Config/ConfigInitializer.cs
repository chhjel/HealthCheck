using HealthCheck.Core.Config;

#if NETFRAMEWORK
using System.Web.Mvc;
#endif

namespace HealthCheck.WebUI.Config
{
    internal static class ConfigInitializer
    {
        /// <summary>
        /// Invoked from <see cref="HCGlobalConfig"/> constructor.
        /// </summary>
        public static void Initialize()
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
            HCGlobalConfig.DefaultInstanceResolver = DependencyResolver.Current.GetService;
#endif
        }
    }
}
