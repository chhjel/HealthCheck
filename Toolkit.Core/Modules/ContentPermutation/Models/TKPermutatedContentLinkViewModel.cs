namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Models
{
    /// <summary>
    /// Model for a single link.
    /// </summary>
    public class TKPermutatedContentLinkViewModel
    {
        /// <summary>
        /// Title of the link.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Where the link points to.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Optional Toolkit access role required to view this link.
        /// </summary>
        public object AccessRoles { get; set; }

        /// <summary>
        /// Model for a single link.
        /// </summary>
        public TKPermutatedContentLinkViewModel(string title, string url)
        {
            Title = title;
            Url = url;
        }
    }
}
