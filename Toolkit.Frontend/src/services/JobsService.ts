import { TKJobSimpleResult } from './../generated/Models/Core/TKJobSimpleResult';
import { TKJobDeleteAllHistoryForJobRequestModel } from './../generated/Models/Core/TKJobDeleteAllHistoryForJobRequestModel';
import { TKJobsStartJobRequestModel } from '@generated/Models/Core/TKJobsStartJobRequestModel';
import { TKJobsStopJobRequestModel } from '@generated/Models/Core/TKJobsStopJobRequestModel';
import { TKJobsGetJobStatusRequestModel } from '@generated/Models/Core/TKJobsGetJobStatusRequestModel';
import { TKJobStatusViewModel } from '@generated/Models/Core/TKJobStatusViewModel';
import { TKJobHistoryEntryViewModel } from '@generated/Models/Core/TKJobHistoryEntryViewModel';
import { TKJobsGetPagedHistoryRequestModel } from '@generated/Models/Core/TKJobsGetPagedHistoryRequestModel';
import { TKJobsGetHistoryDetailRequestModel } from '@generated/Models/Core/TKJobsGetHistoryDetailRequestModel';
import { TKJobDefinitionWithSourceViewModel } from '@generated/Models/Core/TKJobDefinitionWithSourceViewModel';
import { TKPagedJobHistoryEntryViewModel } from '@generated/Models/Core/TKPagedJobHistoryEntryViewModel';
import { TKJobHistoryDetailEntryViewModel } from '@generated/Models/Core/TKJobHistoryDetailEntryViewModel';
import { TKJobStartResultViewModel } from '@generated/Models/Core/TKJobStartResultViewModel';
import { TKJobStopResultViewModel } from '@generated/Models/Core/TKJobStopResultViewModel';
import TKServiceBase, { FetchStatus, ServiceFetchCallbacks } from "./abstractions/TKServiceBase";
import { TKJobDeleteHistoryItemRequestModel } from '@generated/Models/Core/TKJobDeleteHistoryItemRequestModel';

export default class JobsService extends TKServiceBase
{
    public moduleId: string;

    constructor(endpoint: string, inludeQueryString: boolean, moduleId: string)
    {
        super(endpoint, inludeQueryString);
        this.moduleId = moduleId;
    }
    
    public GetJobDefinitions(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKJobDefinitionWithSourceViewModel> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetJobDefinitions", null, statusObject, callbacks);
    }
    
    public GetPagedHistory(
        sourceId: string, jobId: string, pageIndex: number, pageSize: number,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKPagedJobHistoryEntryViewModel | null> | null = null
    ): void {
        const payload: TKJobsGetPagedHistoryRequestModel = {
            SourceId: sourceId,
            JobId: jobId,
            PageIndex: pageIndex,
            PageSize: pageSize
        };
        this.invokeModuleMethod(this.moduleId, "GetPagedHistory", payload, statusObject, callbacks);
    }
    
    public GetPagedLogItems(
        sourceId: string, jobId: string, pageIndex: number, pageSize: number,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKPagedJobHistoryEntryViewModel | null> | null = null
    ): void {
        const payload: TKJobsGetPagedHistoryRequestModel = {
            SourceId: sourceId,
            JobId: jobId,
            PageIndex: pageIndex,
            PageSize: pageSize
        };
        this.invokeModuleMethod(this.moduleId, "GetPagedLogItems", payload, statusObject, callbacks);
    }

    public GetLatestHistoryPerJobId(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKJobHistoryEntryViewModel> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetLatestHistoryPerJobId", null, statusObject, callbacks);
    }
    
    public GetHistoryDetail(
        id: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKJobHistoryDetailEntryViewModel | null> | null = null
    ): void {
        const payload: TKJobsGetHistoryDetailRequestModel = {
            Id: id
        };
        this.invokeModuleMethod(this.moduleId, "GetHistoryDetail", payload, statusObject, callbacks);
    }
    
    public GetJobStatuses(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<Array<TKJobStatusViewModel> | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "GetJobStatuses", null, statusObject, callbacks);
    }
    
    public GetJobStatus(
        sourceId: string, jobId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKJobStatusViewModel | null> | null = null
    ): void {
        const payload: TKJobsGetJobStatusRequestModel = {
            SourceId: sourceId,
            JobId: jobId
        };
        this.invokeModuleMethod(this.moduleId, "GetJobStatus", payload, statusObject, callbacks);
    }
    
    public StartJob(
        sourceId: string, jobId: string, parameters: { [key:string]: string },
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKJobStartResultViewModel | null> | null = null
    ): void {
        const payload: TKJobsStartJobRequestModel = {
            SourceId: sourceId,
            JobId: jobId,
            Parameters: parameters
        };
        this.invokeModuleMethod(this.moduleId, "StartJob", payload, statusObject, callbacks);
    }
    
    public StopJob(
        sourceId: string, jobId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKJobStopResultViewModel | null> | null = null
    ): void {
        const payload: TKJobsStopJobRequestModel = {
            SourceId: sourceId,
            JobId: jobId
        };
        this.invokeModuleMethod(this.moduleId, "StopJob", payload, statusObject, callbacks);
    }

    public DeleteAllHistory(
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKJobSimpleResult | null> | null = null
    ): void {
        this.invokeModuleMethod(this.moduleId, "DeleteAllHistory", null, statusObject, callbacks);
    }
    
    public DeleteAllHistoryForJob(
        sourceId: string, jobId: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKJobSimpleResult | null> | null = null
    ): void {
        const payload: TKJobDeleteAllHistoryForJobRequestModel = {
            SourceId: sourceId,
            JobId: jobId
        };
        this.invokeModuleMethod(this.moduleId, "DeleteAllHistoryForJob", payload, statusObject, callbacks);
    }
    
    public DeleteHistoryItem(
        id: string,
        statusObject: FetchStatus | null = null,
        callbacks: ServiceFetchCallbacks<TKJobSimpleResult | null> | null = null
    ): void {
        const payload: TKJobDeleteHistoryItemRequestModel = {
            Id: id
        };
        this.invokeModuleMethod(this.moduleId, "DeleteHistoryItem", payload, statusObject, callbacks);
    }
}
