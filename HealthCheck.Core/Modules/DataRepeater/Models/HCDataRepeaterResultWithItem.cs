namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Some result data along with a log message.
    /// </summary>
    public class HCDataRepeaterResultWithItem<TData>
    {
        /// <summary></summary>
        public HCDataRepeaterStreamItemViewModel Item { get; set; }

        /// <summary></summary>
        public TData Data {  get; set; }
    }
}
