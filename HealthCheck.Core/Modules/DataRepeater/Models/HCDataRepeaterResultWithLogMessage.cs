namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Some result data along with a log message.
    /// </summary>
    public class HCDataRepeaterResultWithLogMessage<TData>
    {
        /// <summary></summary>
        public TData Data {  get; set; }

        /// <summary></summary>
        public HCDataRepeaterSimpleLogEntry LogMessage { get; set; }
    }
}
