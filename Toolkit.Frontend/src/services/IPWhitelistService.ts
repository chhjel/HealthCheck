import { TKIPWhitelistCidrTest } from './../generated/Models/Module/IPWhitelist/TKIPWhitelistCidrTest';
import { TKIPWhitelistConfig } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistConfig";
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import { TKIPWhitelistRule } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistRule";
import { TKIPWhitelistLogItem } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistLogItem";
import { TKIPWhitelistCheckResult } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistCheckResult";
import { TKIPWhitelistTestRequest } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistTestRequest";
import { TKIPWhitelistLink } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistLink";

export default class IPWhitelistService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }

    public GetLog(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKIPWhitelistLogItem> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetLog", null, statusObject, callbacks);
    }
    
    public GetConfig(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKIPWhitelistConfig | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetConfig", null, statusObject, callbacks);
    }
    
    public SaveConfig(
        payload: TKIPWhitelistConfig,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "SaveConfig", payload, statusObject, callbacks, false);
    }
    
    public GetRules(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKIPWhitelistRule> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetRules", null, statusObject, callbacks);
    }
    
    public SaveRule(
        payload: TKIPWhitelistRule,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKIPWhitelistRule> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "SaveRule", payload, statusObject, callbacks);
    }
    
    public DeleteRule(
        id: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "DeleteRule", id, statusObject, callbacks, false);
    }
    
    public IsRequestAllowed(
        payload: TKIPWhitelistTestRequest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKIPWhitelistCheckResult> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "IsRequestAllowed", payload, statusObject, callbacks);
    }
    
    public IpMatchesCidr(
        payload: TKIPWhitelistCidrTest,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<boolean> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "IpMatchesCidr", payload, statusObject, callbacks);
    }
    
    public DeleteRuleLink(
        linkId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "DeleteRuleLink", linkId, statusObject, callbacks);
    }
    
    public GetRuleLinks(
        ruleId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKIPWhitelistLink>> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetRuleLinks", ruleId, statusObject, callbacks);
    }
    
    public StoreRuleLink(
        link: TKIPWhitelistLink,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKIPWhitelistLink> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "StoreRuleLink", link, statusObject, callbacks);
    }
}
