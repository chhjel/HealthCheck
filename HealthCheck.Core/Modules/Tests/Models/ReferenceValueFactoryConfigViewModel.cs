namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// View model containing options for a reference value factory.
    /// </summary>
    public class ReferenceValueFactoryConfigViewModel
    {
        /// <summary>
        /// Title in the dialog that opens when selecting an item.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description in the dialog that opens when selecting an item.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Text on the search button in the dialog that opens when selecting an item.
        /// </summary>
        public string SearchButtonText { get; set; }
    }
}
