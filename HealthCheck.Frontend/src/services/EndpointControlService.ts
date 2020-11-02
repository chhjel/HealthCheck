import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import { EndpointControlDataViewModel, EndpointControlRule, GenericEndpointControlResult, SetRuleEnabledRequestModel } from "../models/modules/EndpointControl/EndpointControlModels";

export default class EndpointControlService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public SetRuleEnabled(rule: EndpointControlRule, enabled: boolean,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<GenericEndpointControlResult> | null = null
    ): void {
        let payload: SetRuleEnabledRequestModel = {
            RuleId: rule.Id,
            Enabled: enabled
        };
        this.invokeModuleMethod(this.moduleId, 'SetRuleEnabled', payload, statusObject, callbacks);
    }
    
    public DeleteRule(rule: EndpointControlRule,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<GenericEndpointControlResult> | null = null
    ): void {
        let payload = rule.Id;
        this.invokeModuleMethod(this.moduleId, 'DeleteRule', payload, statusObject, callbacks);
    }

    public CreateOrUpdateRule(rule: EndpointControlRule,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<EndpointControlRule> | null = null
    ): void
    {
        this.invokeModuleMethod(this.moduleId, 'CreateOrUpdateRule', rule, statusObject, callbacks);
    }
    
    public GetData(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<EndpointControlDataViewModel> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetData', null, statusObject, callbacks);
    }
    
    public DeleteEndpointDefinition(endpointId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<GenericEndpointControlResult> | null = null
    ): void {
        let payload = endpointId;
        this.invokeModuleMethod(this.moduleId, 'DeleteDefinition', payload, statusObject, callbacks);
    }
    
    public DeleteAllEndpointDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<GenericEndpointControlResult> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'DeleteAllDefinitions', null, statusObject, callbacks);
    }
}
