import { HCModuleIdData } from "generated/Models/Core/HCModuleIdData";
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class AccessTokensService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public DeleteToken(
        id: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TokenData>> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'DeleteToken', id, statusObject, callbacks);
    }
    
    public GetTokens(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TokenData>> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'GetTokens', null, statusObject, callbacks);
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
        callbacks: ServiceFetchCallbacks<CreateNewTokenResponse> | null = null
    ) : void
    {
        this.invokeModuleMethod(this.moduleId, 'CreateNewToken', data, statusObject, callbacks);
    }
}

export interface TokenData
{
    Id: string;
    Name: string;
    CreatedAt: Date;
    CreatedAtSummary: string,
    LastUsedAt: Date | null;
    LastUsedAtSummary: string | null;
    ExpiresAt: Date | null;
    ExpiresAtSummary: string | null;
    IsExpired: boolean;
    Roles: Array<string>;
    Modules: Array<CreatedModuleAccessData>;
}

export interface CreateNewTokenResponse
{
    Id: string;
    Name: string;
    Token: string;
}

export interface CreatedAccessData
{
    Name: string;
    Roles: Array<string>;
    Modules: Array<CreatedModuleAccessData>;
    ExpiresAt: Date | null;
}

export interface CreatedModuleAccessData
{
    ModuleId: string;
    Options: Array<string>;
    Categories: Array<string>;
    Ids: Array<string>;
}

export interface AccessData {
    Roles: Array<AccessRole>;
    ModuleOptions: Array<ModuleAccessData>;
}

export interface ModuleAccessData {
    ModuleName: string;
    ModuleId: string;
    AccessOptions: Array<ModuleAccessOption>;
    AccessCategories: Array<ModuleAccessOption>;
    AccessIds: Array<HCModuleIdData>;
}

export interface ModuleAccessOption {
    Id: string;
    Name: string;
}

export interface AccessRole {
    Id: string;
    Name: string;
}
