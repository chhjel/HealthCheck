namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// Result from a test.
    /// </summary>
    public class TestResultDataDump
    {
        /// <summary>
        /// Name of the data.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Data contents.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Type of data.
        /// </summary>
        public TestResultDataDumpType Type { get; set; }
    }
}
