namespace QoDL.Toolkit.Core.Modules.GoTo.Models
{
    /// <summary></summary>
    public class TKGoToResolvedUrl
    {
        /// <summary>
        /// Name to display.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Where to link.
        /// </summary>
        public string Url { get; set; }

        /// <summary></summary>
        public TKGoToResolvedUrl(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}
