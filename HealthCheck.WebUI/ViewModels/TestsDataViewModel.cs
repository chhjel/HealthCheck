using System.Collections.Generic;

namespace HealthCheck.WebUI.ViewModels
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
    }
}
