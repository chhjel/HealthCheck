
export interface GenericEndpointControlResult {
    Success: boolean;
}

export interface EndpointControlDataViewModel {
    Rules: Array<EndpointControlRule>;
    EndpointDefinitions: Array<EndpointControlEndpointDefinition>;
}

export interface EndpointControlEndpointDefinition {
    EndpointId: string;
    ControllerName: string;
    ActionName: string;
    HttpMethod: string;
}

export interface SetRuleEnabledRequestModel {
    RuleId: string;
    Enabled: boolean;
}

export interface EndpointControlRule {
    Id: string;
    Enabled: boolean;
    LastChangedBy: string;
    LastChangedAt: Date;

    EndpointIdFilter: EndpointControlPropertyFilter;
    UserLocationIdFilter: EndpointControlPropertyFilter;
    UserAgentFilter: EndpointControlPropertyFilter;
    UrlFilter: EndpointControlPropertyFilter;
    
    TotalRequestCountLimits: Array<EndpointControlCountOverDuration>;
    CurrentEndpointRequestCountLimits: Array<EndpointControlCountOverDuration>;
}

export interface EndpointControlPropertyFilter {
    Enabled: boolean;
    Filter: string;
    FilterMode: EndpointControlFilterMode;
    Inverted: boolean;
    CaseSensitive: boolean;
}

export enum EndpointControlFilterMode {
    Matches = 'Matches',
    Contains = 'Contains',
    RegEx = 'RegEx'
}

export interface EndpointControlCountOverDuration {
    Count: number;
    Duration: string;
}
