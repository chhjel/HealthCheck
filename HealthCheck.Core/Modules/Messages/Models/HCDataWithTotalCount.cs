namespace HealthCheck.Core.Modules.Messages.Models
{
    /// <summary>
    /// Contains both a dataset and a total count.
    /// </summary>
    public class HCDataWithTotalCount<T>
    {
        /// <summary>
        /// Data.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Total count.
        /// </summary>
        public int TotalCount { get; set; }
    }

}
