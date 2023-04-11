namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class TKDataRepeaterItemAnalysisResult : TKDataItemChangeBase
    {
        /// <summary>
        /// Message that is returned to the UI when analysis is executed manually on an item.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Optionally set to true to prevent storing the item when analysis is executed before storage.
        /// <para>Allows for optionally calling store on all items and exclude some during analysis.</para>
        /// </summary>
        public bool DontStore { get; set; }
    }
}
