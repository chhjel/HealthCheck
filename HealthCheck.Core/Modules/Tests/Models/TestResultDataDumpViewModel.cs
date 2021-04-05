namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// View model for <see cref="TestResultDataDump"/>
    /// </summary>
    public class TestResultDataDumpViewModel
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
        /// True if data was serialized to json.
        /// </summary>
        public TestResultDataDumpType Type { get; set; }

        /// <summary>
        /// Removes expansion panel and copy/fullscreeen buttons.
        /// </summary>
        public bool? DisplayClean { get; set; }
    }
}
