namespace HealthCheck.Core.Entities
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
        /// True if data was serialized to json.
        /// </summary>
        public bool IsJson { get; set; }
    }
}
