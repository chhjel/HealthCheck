using System;

namespace QoDL.Toolkit.Core.Attributes
{
    /// <summary>Used internally.</summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class TKRtPropertyAttribute : Attribute
    {
        /// <summary></summary>
        public string ForcedType { get; set; }

        /// <summary></summary>
        public bool ForcedNullable { get; set; }
    }
}
