using HealthCheck.Core.Util;
using HealthCheck.Module.EndpointControl.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Always trigger on any matching requests, ignoring any conditions.
        /// </summary>
        public bool AlwaysTrigger { get; set; }

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
        /// Any conditions with parameters.
        /// </summary>
        public List<HCEndpointControlConditionData> Conditions { get; set; } = new List<HCEndpointControlConditionData>();

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
            Func<DateTimeOffset, long> totalRequestCountGetter,
            Func<string, IHCEndpointControlRuleCondition> conditionFactory)
        {
            // Filters are not met => don't block
            if (!AllFiltersMatches(data))
                return false;

            // Always trigger ignores any conditions and limits => block
            if (AlwaysTrigger)
                return true;

            // Conditions are not met => don't block
            if (!AllConditionsMatch(conditionFactory, data))
                return false;

            // Block if limits breached
            return AnyTotalRequestCountLimitBreached(totalRequestCountGetter)
                || AnyEndpointRequestCountLimitBreached(endpointRequestCountGetter);
        }

        private bool AllConditionsMatch(Func<string, IHCEndpointControlRuleCondition> conditionFactory, EndpointControlEndpointRequestData data)
        {
            if (Conditions?.Any() != true) return true;
            foreach (var condition in Conditions)
            {
                var conditionDef = conditionFactory?.Invoke(condition?.ConditionId);
                if (conditionDef != null)
                {
                    object parameters = conditionDef.CustomPropertiesModelType == null ? null : HCValueConversionUtils.ConvertInputModel(conditionDef.CustomPropertiesModelType, condition.Parameters);
                    if (conditionDef.RequestMatchesCondition(data, parameters) == false) return false;
                }
            }
            return true;
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
