using HealthCheck.Core.Config;
using HealthCheck.Module.EndpointControl.Abstractions;
using System;

namespace HealthCheck.Module.EndpointControl.Utils
{
    internal static class EndpointControlServices
    {
        public static IEndpointControlService EndpointControlService => TryGetService<IEndpointControlService>();

        private static T TryGetService<T>() where T : class
        {
            try
            {
                return HCGlobalConfig.GetDefaultInstanceResolver()(typeof(T)) as T;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
