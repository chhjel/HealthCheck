using System;

namespace HealthCheck.Core.Modules.ContentPermutation.Attributes
{
    /// <summary>
    /// Place on your class containing properties to permutate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class HCContentPermutationTypeAttribute : Attribute
    {
        /// <summary>
        /// Optionally override the display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description.
        /// </summary>
        public string Description { get; set; }
    }
}
