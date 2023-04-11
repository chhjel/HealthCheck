namespace QoDL.Toolkit.Core.Modules.Jobs.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class TKJobDefinitionWithSource
    {
        /// <summary></summary>
        public string SourceId { get; set; }

        /// <summary></summary>
        public TKJobDefinition Definition { get; set; }
    }
}
