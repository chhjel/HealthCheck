using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ContentPermutation.Models
{
    /// <summary></summary>
    public class HCPermutatedContentItemViewModel
    {
        /// <summary></summary>
        public string Title { get; set; }

        /// <summary></summary>
        public string MainUrl { get; set; }

        /// <summary></summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional image url.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary></summary>
        public List<HCPermutatedContentLinkViewModel> AdditionalUrls { get; set; }

        /// <summary></summary>
        public HCPermutatedContentItemViewModel(string title, string mainUrl)
        {
            Title = title;
            MainUrl = mainUrl;
        }
    }
}
