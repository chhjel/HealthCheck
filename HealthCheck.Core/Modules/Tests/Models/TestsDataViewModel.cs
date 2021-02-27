using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// Includes data about tests and groups.
    /// </summary>
    public class TestsDataViewModel
    {
        /// <summary>
        /// List of test sets.
        /// </summary>
        public List<TestSetViewModel> TestSets { get; set; }

        /// <summary>
        /// Any options for groups.
        /// </summary>
        public List<GroupOptionsViewModel> GroupOptions { get; set; }

        /// <summary>
        /// A list of any invalid tests.
        /// </summary>
        public List<InvalidTestViewModel> InvalidTests { get; set; }

        /// <summary>
        /// Parameter templates.
        /// </summary>
        public List<TestParameterTemplateViewModel> ParameterTemplateValues { get; set; }
    }
}
