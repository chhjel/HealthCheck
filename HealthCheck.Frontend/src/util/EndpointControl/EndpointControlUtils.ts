import { EndpointControlCountOverDuration } from './../../models/modules/EndpointControl/EndpointControlModels';
import { EndpointControlPropertyFilter, EndpointControlRule } from "../../models/modules/EndpointControl/EndpointControlModels";
import IdUtils from "../IdUtils";

export default class EndpointControlUtils
{
    static describeRule(rule: EndpointControlRule): string {
        return `Rule ${rule.Id} description here`;
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
