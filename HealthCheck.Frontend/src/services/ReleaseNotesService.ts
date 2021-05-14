import { HCReleaseNotesViewModel } from './../generated/Models/Core/HCReleaseNotesViewModel';
import HCServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/HCServiceBase";

export default class ReleaseNotesService extends HCServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetReleaseNotes(
        includeDevDetails: boolean,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<HCReleaseNotesViewModel | null> | null = null
    ): void {
        const method = includeDevDetails ? "GetReleaseNotesWithDevDetails" : "GetReleaseNotesWithoutDevDetails";
        this.invokeModuleMethod(this.moduleId, method, null, statusObject, callbacks);
    }
}
