using HealthCheck.Core.Entities;
using HealthCheck.WebUI.ViewModels;

namespace HealthCheck.WebUI.Factories
{
    /// <summary>
    /// View model object factory for site event objects.
    /// </summary>
    public class SiteEventViewModelsFactory
    {
        /// <summary>
        /// Create a <see cref="SiteEventViewModel"/> from the given <see cref="SiteEvent"/>.
        /// </summary>
        public SiteEventViewModel CreateViewModel(SiteEvent siteEvent)
        {
            var vm = new SiteEventViewModel()
            {
                Severity = siteEvent.Severity,
                SeverityCode = (int)siteEvent.Severity,
                Title = siteEvent.Title,
                Description = siteEvent.Description,
                EventTypeId = siteEvent.EventTypeId,
                Timestamp = siteEvent.Timestamp
            };
            return vm;
        }
    }
}
