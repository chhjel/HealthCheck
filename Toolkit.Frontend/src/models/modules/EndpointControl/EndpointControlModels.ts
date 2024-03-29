import { TKBackendInputConfig } from "@generated/Models/Core/TKBackendInputConfig";
import { TKEndpointControlConditionData } from "@generated/Models/Module/EndpointControl/TKEndpointControlConditionData";
import { TKEndpointControlConditionDefinitionViewModel } from "@generated/Models/Module/EndpointControl/TKEndpointControlConditionDefinitionViewModel";

export interface GenericEndpointControlResult {
    Success: boolean;
}

export interface EndpointControlDataViewModel {
    Rules: Array<EndpointControlRule>;
    EndpointDefinitions: Array<EndpointControlEndpointDefinition>;
    CustomResultDefinitions: Array<EndpointControlCustomResultDefinitionViewModel>;
	Conditions: Array<TKEndpointControlConditionDefinitionViewModel>;
}

export interface EndpointControlCustomResultDefinitionViewModel
{
    Id: string;
    Name: string;
    Description: string;
    CustomProperties: Array<TKBackendInputConfig>;
}

export interface EndpointControlCustomResultPropertyDefinitionViewModel
{
    Id: string;
    Name: string;
    Type: string;
    Description: string;
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
    AlwaysTrigger: boolean;
    LastChangedBy: string;
    LastChangedAt: Date;

    EndpointIdFilter: EndpointControlPropertyFilter;
    UserLocationIdFilter: EndpointControlPropertyFilter;
    UserAgentFilter: EndpointControlPropertyFilter;
    UrlFilter: EndpointControlPropertyFilter;
    
    TotalRequestCountLimits: Array<EndpointControlCountOverDuration>;
    CurrentEndpointRequestCountLimits: Array<EndpointControlCountOverDuration>;
    BlockResultTypeId: string;
    CustomBlockResultProperties: { [key: string]: string };
	Conditions: TKEndpointControlConditionData[];
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
    RegEx = 'RegEx',
    StartsWith = 'StartsWith',
    EndsWith = 'EndsWith'
}

export interface EndpointControlCountOverDuration {
    Count: number;
    Duration: string;
}

export interface EndpointRequestDetails {
    UserLocationIdentifier: string;
    EndpointId: string;
    Timestamp: string; // DateTimeOffset
    UserAgent: string;
    Url: string;
    WasBlocked: boolean;
    BlockingRuleId: string | null;
}

export interface EndpointRequestSimpleDetails {
    EndpointId: string;
    Timestamp: string; // DateTimeOffset
    WasBlocked: boolean;
}
