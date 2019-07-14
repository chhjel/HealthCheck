using HealthCheck.Core.Entities;

namespace HealthCheck.WebUI.ViewModels
{
    /// <summary>
    /// View model for an invalid <see cref="TestDefinition"/>.
    /// </summary>
    public class InvalidTestViewModel
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
        /// Reason why the test is invalid.
        /// </summary>
        public string Reason { get; set; }
    }
}
