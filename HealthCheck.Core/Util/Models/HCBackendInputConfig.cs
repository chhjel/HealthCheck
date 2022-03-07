using HealthCheck.Core.Attributes;
using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Util.Models
{
    /// <summary></summary>
    public class HCBackendInputConfig
    {
        /// <summary></summary>
        public string Type { get; set; }

        /// <summary></summary>
        public string Id { get; set; }

        /// <summary></summary>
        public string Name { get; set; }

        /// <summary></summary>
        public string Description { get; set; }

        /// <summary></summary>
        public bool NotNull { get; set; }

        /// <summary></summary>
        public bool Nullable { get; set; }

        /// <summary></summary>
        public bool FullWidth { get; set; }

        /// <summary></summary>)
        public string DefaultValue { get; set; }

        /// <summary></summary>)
        public string NullName { get; set; }

        /// <summary></summary>
        public List<string> Flags { get; set; }

        /// <summary></summary>
        public List<string> PossibleValues { get; set; }

        /// <summary></summary>
        [HCRtProperty(ForcedType = "number | null")]
        public int? ParameterIndex { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> ExtraValues { get; set; }

        /// <summary></summary>
        internal PropertyInfo PropertyInfo { get; set; }
    }
}
