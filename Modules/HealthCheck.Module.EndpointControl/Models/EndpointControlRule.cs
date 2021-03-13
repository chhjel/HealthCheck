using System;
using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary>
    /// A rule for the endpoint control module.
    /// </summary>
    public class EndpointControlRule
    {
        /// <summary>
        /// Unique id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Disable to ignore this rule.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Name of user that last changed this rule.
        /// </summary>
        public string LastChangedBy { get; set; }

        /// <summary>
        /// Date when last changed.
        /// </summary>
        public DateTimeOffset LastChangedAt { get; set; }

        /// <summary>
        /// Filter for endpoint id value.
        /// </summary>
        public EndpointControlPropertyFilter EndpointIdFilter { get; set; }

        /// <summary>
        /// Filter for user location id value.
        /// </summary>
        public EndpointControlPropertyFilter UserLocationIdFilter { get; set; }

        /// <summary>
        /// Filter for user agent value.
        /// </summary>
        public EndpointControlPropertyFilter UserAgentFilter { get; set; }

        /// <summary>
        /// Filter for url value.
        /// </summary>
        public EndpointControlPropertyFilter UrlFilter { get; set; }

        /// <summary>
        /// Max number of requests from the location id over time duration.
        /// </summary>
        public List<EndpointControlCountOverDuration> TotalRequestCountLimits { get; set; } = new List<EndpointControlCountOverDuration>();

        /// <summary>
        /// Max number of requests from the location id to the current endpoint over time duration.
        /// </summary>
        public List<EndpointControlCountOverDuration> CurrentEndpointRequestCountLimits { get; set; } = new List<EndpointControlCountOverDuration>();

        /// <summary>
        /// Type of custom result when blocked if any.
        /// </summary>
        public string BlockResultTypeId { get; set; }

        /// <summary>
        /// Custom result properties if any.
        /// </summary>
        public Dictionary<string, string> CustomBlockResultProperties { get; set; }

        /// <summary>
        /// Check if the request should be blocked according to the rule data.
        /// </summary>
        public bool ShouldBlockRequest(EndpointControlEndpointRequestData data,
            Func<DateTimeOffset, long> endpointRequestCountGetter,
            Func<DateTimeOffset, long> totalRequestCountGetter)
        {
            if (!AllFiltersMatches(data))
            {
                return false;
            }
            else if (AnyTotalRequestCountLimitBreached(totalRequestCountGetter))
            {
                return true;
            }
            else if (AnyEndpointRequestCountLimitBreached(endpointRequestCountGetter))
            {
                return true;
            }

            return false;
        }

        private bool AllFiltersMatches(EndpointControlEndpointRequestData data)
        {
            if (!Enabled) return false;
            else if (EndpointIdFilter?.Matches(data.EndpointId) == false) return false;
            else if (UserLocationIdFilter?.Matches(data.UserLocationId) == false) return false;
            else if (UserAgentFilter?.Matches(data.UserAgent) == false) return false;
            else if (UrlFilter?.Matches(data.Url) == false) return false;

            return true;
        }

        private bool AnyTotalRequestCountLimitBreached(Func<DateTimeOffset, long> requestCountGetter)
        {
            foreach (var limit in TotalRequestCountLimits)
            {
                var threshold = DateTimeOffset.Now - limit.Duration;
                var requestCount = requestCountGetter(threshold);
                if (requestCount >= limit.Count)
                {
                    return true;
                }
            }

            return false;
        }

        private bool AnyEndpointRequestCountLimitBreached(Func<DateTimeOffset, long> requestCountGetter)
        {
            foreach (var limit in CurrentEndpointRequestCountLimits)
            {
                var threshold = DateTimeOffset.Now - limit.Duration;
                var requestCount = requestCountGetter(threshold);
                if (requestCount >= limit.Count)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
