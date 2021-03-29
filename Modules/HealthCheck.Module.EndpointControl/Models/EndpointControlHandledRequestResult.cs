using HealthCheck.Core.Util;
using HealthCheck.Module.EndpointControl.Abstractions;
using System;
using System.Linq;

namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary>
    /// Result from <see cref="IEndpointControlService.HandleRequest"/>
    /// </summary>
    public class EndpointControlHandledRequestResult
    {
        /// <summary>
        /// True if not rules decided this request should be blocked.
        /// </summary>
        public bool WasDecidedToAllowRequest { get; set; } = true;

        /// <summary>
        /// The rule that decided this request should be blocked.
        /// </summary>
        public EndpointControlRule BlockingRule { get; set; }

        /// <summary>
        /// Selected custom blocked result if any.
        /// </summary>
        public IEndpointControlRequestResult CustomBlockedResult { get; set; }

        /// <summary>
        /// Create a new instance of custom properties to send to the custom blocked result.
        /// </summary>
        public object CreateCustomResultProperties()
        {
            if (CustomBlockedResult == null || CustomBlockedResult?.CustomPropertiesModelType == null)
            {
                return null;
            }

            var instance = HCValueConversionUtils.ConvertInputModel(CustomBlockedResult.CustomPropertiesModelType, BlockingRule?.CustomBlockResultProperties);
            return instance;
        }
    }
}
