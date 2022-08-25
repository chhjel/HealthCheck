using System;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.Modules.ContentPermutation.Attributes
{
    /// <summary>
    /// Optionally place on your property to permutate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class HCContentPermutationPropertyAttribute : Attribute
    {
        /// <summary>
        /// Optional display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Optional description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Get the first attribute on the given property if any.
        /// </summary>
        public static HCContentPermutationPropertyAttribute GetFirst(PropertyInfo property)
            => property?.GetCustomAttributes(typeof(HCContentPermutationPropertyAttribute), true)?.FirstOrDefault() as HCContentPermutationPropertyAttribute;
    }
}
