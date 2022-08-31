namespace HealthCheck.Core.Modules.GoTo.Models
{
    /// <summary></summary>
    public class HCGoToResolvedUrl
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
        public HCGoToResolvedUrl(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}
