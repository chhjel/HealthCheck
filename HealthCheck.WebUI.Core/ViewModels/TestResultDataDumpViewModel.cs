using HealthCheck.Core.Entities;

namespace HealthCheck.Web.Core.ViewModels
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
        public bool IsJson { get; set; }
    }
}
