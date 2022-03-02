namespace HealthCheck.Core.Modules.Dataflow.Models
{
    /// <summary>
    /// Result item from unified search.
    /// </summary>
    public class HCDataflowUnifiedSearchResultItem
    {
        /// <summary>
        /// Title of the result.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Body with html.
        /// </summary>
        public string Body { get; set; }
    }
}
