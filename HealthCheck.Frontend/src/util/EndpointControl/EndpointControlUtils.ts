import { EndpointControlCountOverDuration, EndpointControlFilterMode, EndpointControlEndpointDefinition } from './../../models/modules/EndpointControl/EndpointControlModels';
import { EndpointControlPropertyFilter, EndpointControlRule } from "../../models/modules/EndpointControl/EndpointControlModels";
import IdUtils from "../IdUtils";

export interface RuleDescription
{
    filters: Array<string>;
    limits: Array<string>;
}

export default class EndpointControlUtils
{
    static getEndpointDisplayName(endpointId: string, endpointDefinitions: Array<EndpointControlEndpointDefinition>) : string {
        if (endpointDefinitions == null) return endpointId;

        const def = endpointDefinitions.find(x => x.EndpointId == endpointId);
        if (def == null) return endpointId;

        return `${def.ControllerName}.${def.ActionName} (${def.HttpMethod})`;
    }

    static describeRule(rule: EndpointControlRule): string {
        return `Rule ${rule.Id} description here`;
    }

    static describeRuleExt(rule: EndpointControlRule, endpointDefs: Array<EndpointControlEndpointDefinition>): RuleDescription {
        let filters = [];
        let forcedFilters: Array<string> = [];

        const endpointFilter = rule.EndpointIdFilter;
        if (endpointFilter.Enabled
            && endpointFilter.FilterMode == EndpointControlFilterMode.Matches
            && (
                (endpointFilter.CaseSensitive && endpointDefs.some(x => x.EndpointId == endpointFilter.Filter))
                || (!endpointFilter.CaseSensitive && endpointDefs.some(x => x.EndpointId.toLowerCase() == endpointFilter.Filter.toLowerCase()))
            ))
        {
            const truePart = (endpointFilter.Inverted) ? 'is not' : 'is';
            const endpointName = this.getEndpointDisplayName(endpointFilter.Filter, endpointDefs);
            forcedFilters.push(`Endpoint ${truePart} ${endpointName}`);
        }
        else
        {
            filters.push({ name: 'Endpoint Id', filter: rule.EndpointIdFilter });
        }

        filters = filters.concat([
            { name: 'IP', filter: rule.UserLocationIdFilter },
            { name: 'Url', filter: rule.UrlFilter },
            { name: 'User-Agent', filter: rule.UserAgentFilter }
        ]);

        filters = filters
            .map(x => {
                const desc = this.describeRuleFilter(x.filter);
                return (desc.length == 0) ? '' : `${x.name} ${desc}`;
            })
            .filter(x => x.length > 0);
        
        filters = forcedFilters.concat(filters);

        const limits = 
            rule.TotalRequestCountLimits.map(x => { return { name: 'total request(s) per IP', limit: x } })
            .concat(rule.CurrentEndpointRequestCountLimits.map(x => { return { name: 'request(s) per IP per endpoint', limit: x } }))
            .map(x => this.describeRuleLimit(x.name, x.limit))
            .filter(x => x.length > 0);
        
        return {
            limits: limits,
            filters: filters
        };
    }

    static describeRuleLimit(name: string, limit: EndpointControlCountOverDuration): string {
        if (limit.Count == 0) return `Block all requests`;

        const duration = this.describeTimespan(limit.Duration);
        return `${limit.Count} request over the last ${duration} ${name}.`;
    }

    static describeTimespan(timespan: string): string {
        const parts = timespan.split(':');

        if (parts.length < 3) {
            return '';
        }

        const hourValue = Number(parts[0]);
        const minuteValue = Number(parts[1]);
        const secondValue = Number(parts[2]);

        let finishedParts: Array<string> = [];
        if (hourValue > 0) finishedParts.push(`${hourValue} ${(hourValue == 1 ? 'hour' : 'hours')}`);
        if (minuteValue > 0) finishedParts.push(`${minuteValue} ${(minuteValue == 1 ? 'minute' : 'minutes')}`);
        if (secondValue > 0) finishedParts.push(`${secondValue} ${(secondValue == 1 ? 'second' : 'seconds')}`);

        return finishedParts.joinForSentence(', ', ' and ');
    }

    static describeRuleFilter(filter: EndpointControlPropertyFilter): string {
        if (filter == null || !filter.Enabled
            || filter.Filter == null ||  filter.Filter.length == 0)
        {
            return '';
        }

        let mode = 'contains';
        if (filter.FilterMode == EndpointControlFilterMode.Matches) mode = 'matches';
        else if (filter.FilterMode == EndpointControlFilterMode.RegEx) mode = 'matches regex';

        if (filter.Inverted) {
            mode = 'does not contain';
            if (filter.FilterMode == EndpointControlFilterMode.Matches) mode = 'does not match';
            else if (filter.FilterMode == EndpointControlFilterMode.RegEx) mode = 'does not match regex';
        }

        let suffix = '';
        if (filter.CaseSensitive) suffix = '(case sensitive)';

        return `${mode} '${filter.Filter}'${suffix}`;
    }
    
    static postProcessRule(rule: EndpointControlRule): void {
        rule.LastChangedAt = new Date(rule.LastChangedAt);

        this.postProcessPayloadFilter(rule.EndpointIdFilter);
        this.postProcessPayloadFilter(rule.UserLocationIdFilter);
        this.postProcessPayloadFilter(rule.UserAgentFilter);
        this.postProcessPayloadFilter(rule.UrlFilter);

        rule.CurrentEndpointRequestCountLimits.forEach(x => this.postProcessCountLimit(x));
        rule.TotalRequestCountLimits.forEach(x => this.postProcessCountLimit(x));
    }
    
    static postProcessCountLimit(limit: EndpointControlCountOverDuration): void {
        const anyLimit = <any> limit;
        if (anyLimit._frontendId == null)
        {
            anyLimit._frontendId = IdUtils.generateId();
        }
    }

    static postProcessPayloadFilter(filter: EndpointControlPropertyFilter | null): void {
        if (filter == null) {
            return;
        }

        const anyFilter = <any> filter;
        if (anyFilter._frontendId == null)
        {
            anyFilter._frontendId = IdUtils.generateId();
        }
    }
}
