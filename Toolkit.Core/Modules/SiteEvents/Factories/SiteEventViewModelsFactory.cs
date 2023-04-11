using QoDL.Toolkit.Core.Modules.SiteEvents.Models;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.SiteEvents.Factories
{
    /// <summary>
    /// View model object factory for site event objects.
    /// </summary>
    internal class SiteEventViewModelsFactory
    {
        /// <summary>
        /// Create a <see cref="SiteEventViewModel"/> from the given <see cref="SiteEvent"/>.
        /// </summary>
        public SiteEventViewModel CreateViewModel(SiteEvent siteEvent, bool includeDeveloperDetails)
        {
            var vm = new SiteEventViewModel()
            {
                Id = siteEvent.Id,
                Severity = siteEvent.Severity.ToString(),
                SeverityCode = (int)siteEvent.Severity,
                Title = siteEvent.Title,
                Description = siteEvent.Description,
                Duration = siteEvent.Duration,
                MinimumDurationRequiredToDisplay = siteEvent.MinimumDurationRequiredToDisplay,
                EventTypeId = siteEvent.EventTypeId,
                Timestamp = siteEvent.Timestamp,
                EndTime = siteEvent.Timestamp.AddMinutes(siteEvent.Duration),
                RelatedLinks = siteEvent.RelatedLinks.Select(x => CreateViewModel(x)).ToList(),
                Resolved = siteEvent.Resolved,
                ResolvedMessage = siteEvent.ResolvedMessage,
                ResolvedAt = siteEvent.ResolvedAt,
                DeveloperDetails = (includeDeveloperDetails) ? siteEvent.DeveloperDetails : null
            };
            return vm;
        }

        /// <summary>
        /// Create a <see cref="SiteEventsHyperLinkViewModel"/> from the given <see cref="HyperLink"/>.
        /// </summary>
        public SiteEventsHyperLinkViewModel CreateViewModel(HyperLink link)
        {
            var vm = new SiteEventsHyperLinkViewModel()
            {
                Text = link.Text,
                Url = link.Url
            };
            return vm;
        }
    }
}
