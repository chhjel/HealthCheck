using HealthCheck.Core.Entities;
using System.Collections.Generic;

namespace HealthCheck.WebUI.ViewModels
{
    /// <summary>
    /// View model for a <see cref="TestDefinition"/>.
    /// </summary>
    public class TestViewModel
    {
        /// <summary>
        /// Id of the test.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the test.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the test.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Test parameters.
        /// </summary>
        public List<TestParameterViewModel> Parameters { get; set; }
    }
}
