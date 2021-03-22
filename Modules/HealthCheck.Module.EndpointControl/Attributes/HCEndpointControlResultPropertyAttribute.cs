using System;

namespace HealthCheck.Module.EndpointControl.Attributes
{
    /// <summary>
    /// Optional attribute to describe extra details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HCEndpointControlResultPropertyAttribute : Attribute
    {
        /// <summary>
        /// Any extra description for this property.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Optional attribute to describe extra details.
        /// </summary>
        public HCEndpointControlResultPropertyAttribute(string description = null)
        {
            Description = description;
        }
    }
}
