using Newtonsoft.Json;

namespace HealthCheck.DynamicCodeExecution.Models
{
    /// <summary>
    /// Represents a dump of data.
    /// </summary>
    public class DataDump
    {
        /// <summary>
        /// Title of the dump.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Data type of the dumped value.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The value of the dumped object.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Return the dump in the response or not?
        /// </summary>
        [JsonIgnore]
        public bool Display { get; set; }
    }
}
