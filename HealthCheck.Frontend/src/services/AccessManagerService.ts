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
    
    public CreateNewToken(
        data: CreatedAccessData,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<AccessData> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'CreateNewToken', data, statusObject, callbacks);
    }
}

export interface CreatedAccessData
{
    name: string;
    roles: Array<string>;
    modules: Array<CreatedModuleAccessData>;
}

export interface CreatedModuleAccessData
{
    moduleId: string;
    options: Array<string>;
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
