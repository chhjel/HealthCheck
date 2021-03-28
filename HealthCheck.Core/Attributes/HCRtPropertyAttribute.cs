using System;

namespace HealthCheck.Core.Attributes
{
    /// <summary>Used internally.</summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class HCRtPropertyAttribute : Attribute
    {
        /// <summary></summary>
        public string ForcedType { get; set; }

        /// <summary></summary>
        public bool ForcedNullable { get; set; }
    }
}
