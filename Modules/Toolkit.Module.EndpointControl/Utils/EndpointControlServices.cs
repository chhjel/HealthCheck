using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using System;

namespace QoDL.Toolkit.Module.EndpointControl.Utils
{
    internal static class EndpointControlServices
    {
        public static IEndpointControlService EndpointControlService => TryGetService<IEndpointControlService>();

        private static T TryGetService<T>() where T : class
        {
            try
            {
                return TKGlobalConfig.GetDefaultInstanceResolver()(typeof(T)) as T;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
