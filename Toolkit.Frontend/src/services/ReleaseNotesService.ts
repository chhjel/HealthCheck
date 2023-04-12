import { TKReleaseNotesViewModel } from './../generated/Models/Core/TKReleaseNotesViewModel';
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";

export default class ReleaseNotesService extends TKServiceBase
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
        callbacks: ServiceFetchCallbacks<TKReleaseNotesViewModel | null> | null = null
    ): void {
        const method = includeDevDetails ? "GetReleaseNotesWithDevDetails" : "GetReleaseNotesWithoutDevDetails";
        this.invokeModuleMethod(this.moduleId, method, null, statusObject, callbacks);
    }
}
