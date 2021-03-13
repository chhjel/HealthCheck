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
        public IEndpointControlBlockedRequestResult CustomBlockedResult { get; set; }

        /// <summary>
        /// Create a new instance of custom properties to send to the custom blocked result.
        /// </summary>
        public object CreateCustomResultProperties()
        {
            if (CustomBlockedResult == null || CustomBlockedResult?.CustomPropertiesModelType == null)
            {
                return null;
            }
            else if (BlockingRule?.CustomBlockResultProperties?.Any() != true)
            {
                return Activator.CreateInstance(CustomBlockedResult.CustomPropertiesModelType);
            }

            var instance = Activator.CreateInstance(CustomBlockedResult.CustomPropertiesModelType);
            var props = CustomBlockedResult.CustomPropertiesModelType.GetProperties();
            foreach (var prop in props)
            {
                var stringValue = BlockingRule?.CustomBlockResultProperties?.FirstOrDefault(x => x.Key == prop.Name).Value;
                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(instance, stringValue);
                }
                else if (prop.PropertyType == typeof(int) && int.TryParse(stringValue, out int intValue))
                {
                    prop.SetValue(instance, intValue);
                }
                else if (prop.PropertyType == typeof(bool) && bool.TryParse(stringValue, out bool boolValue))
                {
                    prop.SetValue(instance, boolValue);
                }
            }
            return instance;
        }
    }
}
