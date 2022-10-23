using System;

namespace HealthCheck.Core.Models
{
    /// <summary></summary>
    public class HCTypeNamePair
    {
        /// <summary></summary>
        public Type Type { get; set; }
        /// <summary></summary>
        public string Name { get; set; }
        /// <summary></summary>
        public HCTypeNameNamePair ToNamed() => new() { Name = Name, TypeName = Type?.Name };
    }
}
