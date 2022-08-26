using HealthCheck.Core.Modules.ContentPermutation.Models;
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

        /// <summary>
        /// Requested <see cref="HCGetContentPermutationContentOptions.MaxCount"/> will be limited by this number. Defaults to 8.
        /// </summary>
        public int MaxAllowedContentCount { get; set; } = 8;

        /// <summary>
        /// Default value for <see cref="HCGetContentPermutationContentOptions.MaxCount"/> will be limited by this number. Defaults to 8.
        /// </summary>
        public int DefaultContentCount { get; set; } = 8;
    }
}
