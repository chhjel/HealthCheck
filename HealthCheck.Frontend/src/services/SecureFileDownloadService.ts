import { HCSecureFileDownloadFileDetails } from './../generated/Models/Core/HCSecureFileDownloadFileDetails';
import HCServiceBase, { FetchStatus, FetchStatusWithProgress, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";
import { SecureFileDownloadsViewModel, SecureFileDownloadDefinition, SecureFileDownloadSaveViewModel, SecureFileDownloadStorageUploadFileResult } from "../models/modules/SecureFileDownload/Models";
import UrlUtils from '@util/UrlUtils';
import SecureFileDownloadUtils from '@util/SecureFileDownload/SecureFileDownloadUtils';

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
    
    public DeleteUploadedFile(definitionId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<boolean> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, 'DeleteUploadedFile', definitionId, statusObject, callbacks);
    }

    public UploadFile(file: File, fileDefId: string, storageId: string, endpointBase: string, statusObject: FetchStatusWithProgress | null = null)
        : Promise<SecureFileDownloadStorageUploadFileResult> {

        return new Promise((resolve, reject) => {
            if (statusObject) statusObject.inProgress = true;

            try {
                const req = new XMLHttpRequest();
            
                req.upload.addEventListener('progress', (event: ProgressEvent) => {
                    if (event.lengthComputable) {
                        if (statusObject) statusObject.progress = event.loaded / event.total * 100;
                    }
                });
                req.upload.addEventListener('load', (event: Event) => {
                    if (statusObject) statusObject.progress = 100;
                });
                req.upload.addEventListener('error', (event: ProgressEvent<XMLHttpRequestEventTarget>) => {
                    if (statusObject) {
                        statusObject.progress = 0;
                        statusObject.errorMessage = `Failed to upload.`;
                    }
                    const result: SecureFileDownloadStorageUploadFileResult = {
                        success: false,
                        fileId: '',
                        message: 'Failed to upload.',
                        defId: ''
                    };
                    resolve(result);
                });
                req.onload  = function() {
                   var jsonResponse = req.response;
                   const result: SecureFileDownloadStorageUploadFileResult = JSON.parse(jsonResponse);
                   resolve(result);
                };
            
                const formData = new FormData();
                formData.append('file', file, file.name);
            
                endpointBase
                let url = SecureFileDownloadUtils.getUploadUrl(endpointBase);
                req.open('POST', `${url}`);
                req.setRequestHeader('x-id', fileDefId);
                req.setRequestHeader('x-storage-id', storageId);
                req.send(formData);
            }
            catch(e) {
                if (statusObject) statusObject.errorMessage = `Failed to upload. ${e}`;
            }
            
            if (statusObject) statusObject.inProgress = false;
        });
      }
}
