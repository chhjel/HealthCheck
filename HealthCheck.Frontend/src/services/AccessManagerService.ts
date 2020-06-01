import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class AccessManagerService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetAccessData(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<AccessData> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetAccessData', null, statusObject, callbacks);
    }
}

export interface AccessData {
    Roles: Array<AccessRole>;
    ModuleOptions: Array<ModuleAccessData>;
}

export interface ModuleAccessData {
    ModuleName: string;
    ModuleId: string;
    AccessOptions: Array<ModuleAccessOption>;
}

export interface ModuleAccessOption {
    Id: string;
    Name: string;
}

export interface AccessRole {
    Id: string;
    Name: string;
}
