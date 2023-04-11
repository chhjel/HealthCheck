namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Some result data along with a log message.
    /// </summary>
    public class TKDataRepeaterResultWithItem<TData>
    {
        /// <summary></summary>
        public TKDataRepeaterStreamItemViewModel Item { get; set; }

        /// <summary></summary>
        public TData Data {  get; set; }
    }
}
