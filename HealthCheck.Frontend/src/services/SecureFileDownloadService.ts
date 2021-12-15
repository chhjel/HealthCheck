import { HCSecureFileDownloadFileDetails } from './../generated/Models/Core/HCSecureFileDownloadFileDetails';
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import { SecureFileDownloadsViewModel, SecureFileDownloadDefinition, SecureFileDownloadSaveViewModel } from "../models/modules/SecureFileDownload/Models";

export default class SecureFileDownloadService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetDownloads(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<SecureFileDownloadsViewModel> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'GetDownloads', null, statusObject, callbacks);
    }
    
    public GetStorageFileIdOptions(
        storageId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<HCSecureFileDownloadFileDetails>> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'GetStorageFileIdOptions', storageId, statusObject, callbacks);
    }
    
    public SaveDefinition(definition: SecureFileDownloadDefinition,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<SecureFileDownloadSaveViewModel> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'SaveDefinition', definition, statusObject, callbacks);
    }
    
    public DeleteDefinition(definitionId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<boolean> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'DeleteDefinition', definitionId, statusObject, callbacks);
    }
}
