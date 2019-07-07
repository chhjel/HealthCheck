namespace RuntimeCodeTest.Core.Entities
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
    }
}
