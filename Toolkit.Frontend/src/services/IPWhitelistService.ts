import { TKIPWhitelistConfig } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistConfig";
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import { TKIPWhitelistRule } from "@generated/Models/Module/IPWhitelist/TKIPWhitelistRule";

export default class IPWhitelistService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
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
        this.invokeModuleMethod(this.moduleId, "SaveConfig", payload, statusObject, callbacks);
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
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "SaveRule", payload, statusObject, callbacks);
    }
    
    public DeleteRule(
        id: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<any> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "DeleteRule", id, statusObject, callbacks);
    }
}