<!-- src/components/modules/Jobs/JobComponent.vue -->
<template>
    <div class="job-component">
        <icon-component :color="jobIconColor()">{{ jobIcon() }}</icon-component>
        <h2>{{ job.Definition.Name }}</h2>
        <p v-if="job.Definition.Description">{{ job.Definition.Description }}</p>
        <div v-if="status">
            <code>IsEnabled:{{status.IsEnabled}}</code><br />
            <code>IsRunning:{{status.IsRunning}}</code><br />
            <code>Summary:{{status.Summary}}</code><br />
            <code>NextExecutionScheduledAt:{{status.NextExecutionScheduledAt}}</code><br />
            <code>StartedAt:{{status.StartedAt}}</code><br />
            <code>EndedAt:{{status.EndedAt}}</code><br />
            <code>LastRunWasSuccessful:{{status.LastRunWasSuccessful}}</code>
            <progress-linear-component v-if="isJobRunning"
                indeterminate
                height="4"
                color="primary"
            />
            <div v-else style="height: 4px"></div>
        </div>

        <btn-component v-if="showStartJobButton"
            :disabled="isLoading || isJobRunning"
            @click="startJob"
            color="primary"
            >Start job</btn-component>
        <btn-component v-if="showStopJobButton"
            :disabled="isLoading || !isJobRunning"
            @click="stopJob"
            color="primary"
            >Stop job</btn-component>
        <btn-component v-if="showRefreshButton"
            :disabled="isLoading"
            @click="refresh"
            color="secondary"
            >Refresh results</btn-component>

        <div v-if="pagedHistory && pagedHistory.Items.length > 0">
            <paging-component
                :count="totalHistoryCount"
                :pageSize="pageSize"
                v-model:value="pageIndex"
                @change="onPageIndexChanged"
                :disabled="isLoading"
                :asIndex="true"
                class="mb-2 mt-2"
                />
                
            <div v-for="entry in pagedHistory.Items"
                :key="`historyEntry-${entry.SourceId}-${entry.Id}`">
                <icon-component :color="jobHistoryIconColor(entry)">{{ jobHistoryIcon(entry) }}</icon-component>
                <code
                    :class="{ clickable: hasAccessToViewJobHistoryDetails }"
                    @click="loadHistoryDetailsFor(entry)"
                    >{{entry}}</code>
            </div>
        
            <paging-component
                :count="totalHistoryCount"
                :pageSize="pageSize"
                v-model:value="pageIndex"
                @change="onPageIndexChanged"
                :disabled="isLoading"
                :asIndex="true"
                class="mb-2 mt-2"
                />
        </div>
        <div v-if="!pagedHistory || pagedHistory.TotalCount == 0" class="no-results-text">Job has no extended results yet.</div>

        <dialog-component v-model:value="loadDetailsDialogVisible"
            fullscreen
            :dark="historyDetails?.DataIsHtml !== true"
            :persistent="detailLoadProgress.inProgress"
            :smartPersistent="false">
            <template #header>{{detailsDialogTitle}}</template>
            <template #footer>
                <btn-component color="secondary"
                    :disabled="detailLoadProgress.inProgress"
                    :loading="detailLoadProgress.inProgress"
                    @click="loadDetailsDialogVisible = false">Close</btn-component>
            </template>
            <div v-if="historyDetails" style="height: 100%">
                <progress-linear-component 
                    v-if="detailLoadProgress.inProgress"
                    indeterminate color="success"></progress-linear-component>
                <div v-if="historyDetails.DataIsHtml" v-html="historyDetails.Data"></div>
                <div v-if="!historyDetails.DataIsHtml" style="height: 100%">
                    <editor-component
                        class="editor"
                        :language="'json'"
                        v-model:value="historyDetails.Data"
                        :read-only="true"
                        ref="editor" />
                </div>
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import { StoreUtil } from "@util/StoreUtil";
import JobsService from "@services/JobsService";
import IdUtils from "@util/IdUtils";
import { HCJobDefinitionWithSourceViewModel } from "@generated/Models/Core/HCJobDefinitionWithSourceViewModel";
import { HCJobHistoryDetailEntryViewModel } from "@generated/Models/Core/HCJobHistoryDetailEntryViewModel";
import { HCPagedJobHistoryEntryViewModel } from "@generated/Models/Core/HCPagedJobHistoryEntryViewModel";
import { HCJobStatusViewModel } from "@generated/Models/Core/HCJobStatusViewModel";
import PagingComponent from "@components/Common/Basic/PagingComponent.vue";
import EditorComponent from "@components/Common/EditorComponent.vue";
import DateUtils from "@util/DateUtils";
import { HCJobHistoryEntryViewModel } from "@generated/Models/Core/HCJobHistoryEntryViewModel";
import JobUtils from "./JobUtils";

@Options({
    components: {
        PagingComponent,
        EditorComponent
    }
})
export default class JobComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    @Prop({ required: true })
    job: HCJobDefinitionWithSourceViewModel;

    @Prop({ required: true })
    status: HCJobStatusViewModel | null;
    
    @Ref() readonly editor!: EditorComponent;

    // Service
    service: JobsService = new JobsService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();
    detailLoadProgress: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    pagedHistory: HCPagedJobHistoryEntryViewModel | null = null;
    latestHistory: HCJobHistoryEntryViewModel | null = null;
    historyDetails: HCJobHistoryDetailEntryViewModel | null = null;
    historyDetailsParent: HCJobHistoryEntryViewModel | null = null;

    // Pagination
    totalHistoryCount: number = 0;
    pageSizeDefault: number = 4;
    pageIndex: number = 0;
    pageSize: number = this.pageSizeDefault;

    // Dialogs
    loadDetailsDialogVisible: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadPagedHistory(true);
        
        this.refreshEditorSize();
        this.$nextTick(() => this.refreshEditorSize());
        setTimeout(() => {
            this.refreshEditorSize();
        }, 100);
    }

    ////////////////
    //  METHODS  //
    //////////////
    refresh(): void {
        this.loadPagedHistory(false);
    }

    loadPagedHistory(resetPageIndex: boolean): void {
        if (!this.hasAccessToViewJobHistory) return;

        if (resetPageIndex) {
            this.pageIndex = 0;
        }
        let pageIndex = this.pageIndex;
        this.service.GetPagedHistory(this.job.SourceId, this.job.Definition.Id, this.pageIndex, this.pageSize, this.dataLoadStatus, {
            onSuccess: (data) => {
                this.pagedHistory = data;
                this.totalHistoryCount = data.TotalCount;

                if (pageIndex == 0) {
                    this.latestHistory = data.Items[0];
                }
            }
        });
    }

    loadHistoryDetailsFor(history: HCJobHistoryEntryViewModel): void {
        if (!this.hasAccessToViewJobHistoryDetails) return;

        this.historyDetailsParent = history;
        this.loadHistoryDetails(history.DetailId);
    }

    loadHistoryDetails(id: string): void {
        this.service.GetHistoryDetail(id, this.detailLoadProgress, {
            onSuccess: (data) => {
                this.historyDetails = data;
                this.loadDetailsDialogVisible = true;
            }
        });
    }

    startJob(): void {
        const sourceId = this.job.SourceId;
        const jobId = this.job.Definition.Id;
        this.service.StartJob(sourceId, jobId, this.dataLoadStatus, {
            onSuccess: (data) => this.updateJobRunningStatus(sourceId, jobId, data.Success, true, data.Message)
        });
    }

    stopJob(): void {
        const sourceId = this.job.SourceId;
        const jobId = this.job.Definition.Id;
        this.service.StopJob(sourceId, jobId, this.dataLoadStatus, {
            onSuccess: (data) => this.updateJobRunningStatus(sourceId, jobId, data.Success, false, data.Message)
        });
    }

    updateJobRunningStatus(sourceId: string, jobId: string, success: boolean, running: boolean, message: string): void {
        this.$emit("updateJobRunningStatus", sourceId, jobId, success, running, message);
    }

    refreshEditorSize(): void {
        if (this.editor)
        {
            this.editor.refreshSize();
        }
    }

    hasAccess(option: string): boolean {
        return this.options.AccessOptions.indexOf(option) != -1;
    }

    jobIcon(): string { return JobUtils.jobIcon(this.status, this.latestHistory); };
    jobIconColor(): string { return JobUtils.jobIconColor(this.status, this.latestHistory); };
    jobHistoryIcon(history: HCJobHistoryEntryViewModel): string { return JobUtils.jobIcon(null, history); };
    jobHistoryIconColor(history: HCJobHistoryEntryViewModel): string { return JobUtils.jobIconColor(null, history); };

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get isLoading(): boolean {
        return this.dataLoadStatus.inProgress;
    }

    get isJobRunning(): boolean | null {
        return this.status?.IsRunning == true;
    }

    get detailsDialogTitle(): string {
        if (!this.historyDetails || !this.historyDetailsParent) return '';
        const timestamp = new Date(this.historyDetailsParent.Timestamp);
        const time = DateUtils.FormatDate(timestamp, 'd. MMM HH:mm:ss');
        return `${time}`;
    }

    get showStartJobButton(): boolean { return this.hasAccessToStartJob && this.job.Definition.SupportsStart; }
    get showStopJobButton(): boolean { return this.hasAccessToStopJob && this.job.Definition.SupportsStop; }
    get showRefreshButton(): boolean { return this.hasAccessToViewJobHistory; }

    get hasAccessToStartJob(): boolean { return this.hasAccess('StartJob'); }
    get hasAccessToStopJob(): boolean { return this.hasAccess('StopJob'); }
    get hasAccessToViewJobHistory(): boolean { return this.hasAccess('ViewJobHistory'); }
    get hasAccessToViewJobHistoryDetails(): boolean { return this.hasAccess('ViewJobHistoryDetails'); }
    get hasAccessToDeleteHistory(): boolean { return this.hasAccess('DeleteHistory'); }
    get hasAccessToDeleteAllHistory(): boolean { return this.hasAccess('DeleteAllHistory'); }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onPageIndexChanged(): void {
        this.loadPagedHistory(false);
    }
}
</script>

<style scoped lang="scss">
.job-component {

}
.no-results-text {
    padding: 10px;
    font-weight: 600;
}
.editor {
  width: 100%;
  height: 100%;
}
</style>
