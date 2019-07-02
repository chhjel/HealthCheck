using HealthCheck.Core.Entities;
using System.Collections.Generic;

namespace HealthCheck.Web.Core.ViewModels
{
    /// <summary>
    /// View model for a <see cref="TestClassDefinition"/>.
    /// </summary>
    public class TestSetViewModel
    {
        /// <summary>
        /// Id of the test set.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the test set.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the test set.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Optional icon name override. Get name from https://material.io/tools/icons/.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Order of the set in the ui lists, higher value = higher up.
        /// </summary>
        public int UIOrder { get; set; }

        /// <summary>
        /// List of tests in the set.
        /// </summary>
        public List<TestViewModel> Tests { get; set; }
    }
}
