using HealthCheck.Core.Entities;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.WebUI.ViewModels;
using System.Linq;

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
        public SiteEventViewModel CreateViewModel(SiteEvent siteEvent, bool includeDeveloperDetails)
        {
            var vm = new SiteEventViewModel()
            {
                Id = siteEvent.Id,
                Severity = siteEvent.Severity,
                SeverityCode = (int)siteEvent.Severity,
                Title = siteEvent.Title,
                Description = siteEvent.Description,
                Duration = siteEvent.Duration,
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
        /// Create a <see cref="HyperLinkViewModel"/> from the given <see cref="HyperLink"/>.
        /// </summary>
        public HyperLinkViewModel CreateViewModel(HyperLink link)
        {
            var vm = new HyperLinkViewModel()
            {
                Text = link.Text,
                Url = link.Url
            };
            return vm;
        }
    }
}
