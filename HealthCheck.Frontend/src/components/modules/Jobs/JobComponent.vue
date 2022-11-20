<!-- src/components/modules/Jobs/JobComponent.vue -->
<template>
    <div class="job-component">
        <div class="job__header">
            <icon-component :color="jobIconColor()">{{ jobIcon() }}</icon-component>
            <h2>{{ job.Definition.Name }}</h2>
        </div>
        <p v-if="job.Definition.Description">{{ job.Definition.Description }}</p>

        <div>
            <btn-component v-if="showStartJobButton"
                :disabled="disableStartJobButton"
                @click="startJob"
                color="primary"
                >{{ startJobButtonText }}</btn-component>
            <btn-component v-if="showStartJobWithParametersButton"
                :disabled="disableStartJobWithParametersButton"
                @click="startWithParametersDialogVisible = true"
                color="primary"
                >Start job with parameters..</btn-component>
            <btn-component v-if="showStopJobButton"
                :disabled="disableStopJobButton"
                @click="stopJob"
                color="error"
                >{{ stopJobButtonText }}</btn-component>
            <btn-component v-if="showRefreshButton"
                :disabled="isLoading"
                @click="refresh"
                color="secondary"
                >Refresh results</btn-component>
        </div>
        <progress-linear-component v-if="isJobRunning"
            indeterminate
            height="4"
            color="primary"
        />
        <div v-else style="height: 4px"></div>

        <div v-if="status" class="job__status">
            <ul style="min-height: 62px">
                <li v-if="runningStatusText" v-html="runningStatusText"></li>
                <li v-if="nextExecStatusText" v-html="nextExecStatusText"></li>
                <li v-if="enabledStatusText" v-html="enabledStatusText"></li>
                <li v-if="statusText" v-html="statusText"></li>
            </ul>
        </div>

        <div v-if="pagedHistory && pagedHistory.Items.length > 0" class="mt-4">
            <h4>Extended job results</h4>
            <paging-component
                :count="totalHistoryCount"
                :pageSize="pageSize"
                v-model:value="pageIndex"
                @change="onPageIndexChanged"
                :disabled="isLoading"
                :asIndex="true"
                class="mb-2 mt-2"
                />
            
            <div>
                <div v-for="entry in pagedHistory.Items"
                    :key="`historyEntry-${entry.SourceId}-${entry.Id}`"
                    class="history-entry"
                    :class="{ clickable: hasAccessToViewJobHistoryDetails }"
                    @click="loadHistoryDetailsFor(entry)">
                    <div class="history-entry__header">
                        <icon-component :color="jobHistoryIconColor(entry)" class="mr-1">{{ jobHistoryIcon(entry) }}</icon-component>
                        <div v-html="pagedEntryTitle(entry)"></div>
                    </div>
                    <div class="history-entry__summary">{{ entry.Summary }}</div>
                </div>
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
        <div v-if="pagedLogItems && pagedLogItems.Items.length > 0" class="mt-4">
            <h4>Job results</h4>
            <paging-component
                :count="logTotalHistoryCount"
                :pageSize="logPageSize"
                v-model:value="logPageIndex"
                @change="onLogPageIndexChanged"
                :disabled="isLoading"
                :asIndex="true"
                class="mb-2 mt-2"
                />
            
            <div>
                <div v-for="entry in pagedLogItems.Items"
                    :key="`logEntry-${entry.SourceId}-${entry.Id}-${entry.Timestamp}`"
                    class="log-entry">
                    <div class="log-entry__header">
                        <icon-component :color="jobHistoryIconColor(entry)" class="mr-1">{{ jobHistoryIcon(entry) }}</icon-component>
                        <div v-html="pagedEntryTitle(entry)"></div>
                    </div>
                    <div class="log-entry__summary">{{ entry.Summary }}</div>
                </div>
            </div>
        
            <paging-component
                :count="logTotalHistoryCount"
                :pageSize="logPageSize"
                v-model:value="logPageIndex"
                @change="onLogPageIndexChanged"
                :disabled="isLoading"
                :asIndex="true"
                class="mb-2 mt-2"
                />
        </div>

        <!-- DIALOGS -->
        <dialog-component v-model:value="loadDetailsDialogVisible"
            :dark="historyDetails?.DataIsHtml !== true"
            :persistent="detailLoadProgress.inProgress"
            :smartPersistent="false"
            width="90%"
            max-width="90%">
            <template #header>
                <div v-if="historyDetailsParent"
                    v-html="detailDialogTitle(historyDetailsParent)"></div>
            </template>
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
                <div v-if="historyDetails.DataIsHtml"
                    v-html="historyDetails.Data"
                    :key="historyDetails.Id"></div>
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
        <dialog-component v-model:value="startWithParametersDialogVisible">
            <template #header>Start job with parameters</template>
            <template #footer>
                <btn-component color="primary"
                    v-if="showStartJobWithParametersButton"
                    :disabled="disableStartJobWithParametersButton"
                    @click="startJobWithParametersClicked"
                    >{{ startJobButtonText }}</btn-component>
                <btn-component color="secondary"
                    :disabled="detailLoadProgress.inProgress"
                    :loading="detailLoadProgress.inProgress"
                    @click="startWithParametersDialogVisible = false">Close</btn-component>
            </template>
            <div v-if="customParameterDefinitions != null">
                <div class="job-parameter-items">
                    <backend-input-component
                        v-for="(parameterDef, pIndex) in customParameterDefinitions"
                        :key="`job-parameter-item-${job.Id}-${pIndex}`"
                        class="job-parameter-item"
                        :class="jobParameterItemClasses(parameterDef)"
                        v-model:value="customParameters[parameterDef.Id]"
                        :config="parameterDef"
                        :readonly="isLoading"
                        />
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
import { HCBackendInputConfig } from "@generated/Models/Core/HCBackendInputConfig";
import { HCUIHint } from "@generated/Enums/Core/HCUIHint";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";

@Options({
    components: {
        PagingComponent,
        EditorComponent,
        BackendInputComponent
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
    pagedLogItems: HCPagedJobHistoryEntryViewModel | null = null;
    latestHistory: HCJobHistoryEntryViewModel | null = null;
    latestLogItem: HCJobHistoryEntryViewModel | null = null;
    historyDetails: HCJobHistoryDetailEntryViewModel | null = null;
    historyDetailsParent: HCJobHistoryEntryViewModel | null = null;

    // Pagination
    totalHistoryCount: number = 0;
    pageSizeDefault: number = 10;
    pageIndex: number = 0;
    pageSize: number = this.pageSizeDefault;
    logTotalHistoryCount: number = 0;
    logPageSizeDefault: number = 10;
    logPageIndex: number = 0;
    logPageSize: number = this.pageSizeDefault;

    isStarting: boolean = false;
    isStopping: boolean = false;

    // Dialogs
    loadDetailsDialogVisible: boolean = false;
    startWithParametersDialogVisible: boolean = false;
    customParameters: { [key:string]: string } = {};

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadPagedHistory(true);
        this.loadPagedLogItems(true);
        
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
        this.loadPagedLogItems(false);
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

                // DEV
                // if (this.pagedHistory.Items.length > 0) {
                //     this.loadHistoryDetailsFor(this.pagedHistory.Items[0]);
                // }
                //
            }
        });
    }

    loadPagedLogItems(resetPageIndex: boolean): void {
        if (!this.hasAccessToViewJobHistory) return;

        if (resetPageIndex) {
            this.logPageIndex = 0;
        }
        let pageIndex = this.logPageIndex;
        this.service.GetPagedLogItems(this.job.SourceId, this.job.Definition.Id, this.logPageIndex, this.logPageSize, this.dataLoadStatus, {
            onSuccess: (data) => {
                this.pagedLogItems = data;
                this.logTotalHistoryCount = data.TotalCount;

                if (pageIndex == 0) {
                    this.latestLogItem = data.Items[0];
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

    startJobWithParametersClicked(): void {
        this.startJob(true);
        this.startWithParametersDialogVisible = false;
    }

    startJob(includeParameters: boolean = false): void {
        const sourceId = this.job.SourceId;
        const jobId = this.job.Definition.Id;
        const parameters: { [key:string]: string } = includeParameters == true ? this.customParameters : {};
        this.service.StartJob(sourceId, jobId, parameters, this.dataLoadStatus, {
            onSuccess: (data) => this.isStarting = data.Success
        });
    }

    stopJob(): void {
        const sourceId = this.job.SourceId;
        const jobId = this.job.Definition.Id;
        this.service.StopJob(sourceId, jobId, this.dataLoadStatus, {
            onSuccess: (data) => this.isStopping = data.Success
        });
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

    pagedEntryTitle(entry: HCJobHistoryEntryViewModel): string {
        if (entry.StartedAt != null) {
            const elapsedMs = new Date(entry.EndedAt).getTime() - new Date(entry.StartedAt).getTime();
            const duration = DateUtils.prettifyDurationString(elapsedMs, '', '0 seconds');
            return `${this.detailCodeTagStart}${this.formatDateTime(entry.StartedAt)}</code> to ${this.detailCodeTagStart}${this.formatDateTime(entry.EndedAt)}</code>. Ran for ${this.detailCodeTagStart}${duration}</code>.`;
        }
        else return this.formatDateTime(entry.EndedAt);
    }

    detailDialogTitle(entry: HCJobHistoryEntryViewModel): string {
        if (entry.StartedAt != null) {
            const elapsedMs = new Date(entry.EndedAt).getTime() - new Date(entry.StartedAt).getTime();
            const duration = DateUtils.prettifyDurationString(elapsedMs, '', '0 seconds');
            return `${this.formatDateTime(entry.StartedAt)} to ${this.formatDateTime(entry.EndedAt)}. Ran for ${duration}.`;
        }
        else return this.formatDateTime(entry.EndedAt);
    }

    formatTimeOnly(date: Date | string): string {
        return DateUtils.FormatDate(date, 'HH:mm:ss');
    }

    formatDateTime(date: Date | string): string {
        return DateUtils.FormatDate(date, 'd. MMM HH:mm:ss');
    }
    
    jobParameterItemClasses(parameter: HCBackendInputConfig): any {
        let classes: any = {
            'full-width': parameter.UIHints.includes(HCUIHint.FullWidth)
        };
        return classes;
    }

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

    get showStartJobButton(): boolean { return this.hasAccessToStartJob && this.job.Definition.SupportsStart; }
    get showStartJobWithParametersButton(): boolean {
        return this.showStartJobButton
            && this.hasAccessToStartJobWithParameters
            && this.job?.Definition?.HasCustomParameters == true;
    }
    get showStopJobButton(): boolean { return this.hasAccessToStopJob && this.job.Definition.SupportsStop && this.isJobRunning; }
    get showRefreshButton(): boolean { return this.hasAccessToViewJobHistory; }
    get startJobButtonText(): string { return this.isJobRunning ? 'Running..' : (this.isStarting ? "Starting.." : "Start job"); }
    get stopJobButtonText(): string { return this.isStopping ? "Requested stop.." : "Stop job"; }
    get disableStartJobButton(): boolean { return this.isStarting || this.isLoading || this.isJobRunning; }
    get disableStartJobWithParametersButton(): boolean { return this.isStarting || this.isLoading || this.isJobRunning; }
    get disableStopJobButton(): boolean { return this.isStopping || this.isLoading || !this.isJobRunning; }

    get hasAccessToStartJob(): boolean { return this.hasAccess('StartJob'); }
    get hasAccessToStartJobWithParameters(): boolean { return this.hasAccessToStartJob && this.hasAccess('StartJobWithCustomParameters'); }
    get hasAccessToStopJob(): boolean { return this.hasAccess('StopJob'); }
    get hasAccessToViewJobHistory(): boolean { return this.hasAccess('ViewJobHistory'); }
    get hasAccessToViewJobHistoryDetails(): boolean { return this.hasAccess('ViewJobHistoryDetails'); }
    get hasAccessToDeleteHistory(): boolean { return this.hasAccess('DeleteHistory'); }
    get hasAccessToDeleteAllHistory(): boolean { return this.hasAccess('DeleteAllHistory'); }
    get customParameterDefinitions(): HCBackendInputConfig[] { return this.job.Definition.CustomParameters; }

    detailCodeTagStart: string = '<code class="inline-detail">';
    get runningStatusText(): string {
        if (this.status.IsRunning) {
            // const elapsedMs = new Date().getTime() - new Date(this.status.StartedAt).getTime();
            // const duration = DateUtils.prettifyDurationString(elapsedMs, '', '0 seconds');
            // return `Job is running.. Started at ${this.detailCodeTagStart}${this.formatTimeOnly(this.status.StartedAt)}</code> and has been running for ${this.detailCodeTagStart}${duration}</code>.`;
            return `Job is running..`;
        }
        else if (this.status.StartedAt != null && this.status.EndedAt != null) {
            const elapsedMs = new Date(this.status.EndedAt).getTime() - new Date(this.status.StartedAt).getTime();
            const duration = DateUtils.prettifyDurationString(elapsedMs, '', '0 seconds');
            return `Job last ran from ${this.detailCodeTagStart}${this.formatTimeOnly(this.status.StartedAt)}</code> to ${this.detailCodeTagStart}${this.formatTimeOnly(this.status.StartedAt)}</code> and used ${this.detailCodeTagStart}${duration}</code>.`;
        }
        else return '';
    }

    get nextExecStatusText(): string {
        if (this.status.IsRunning || !this.status.IsEnabled) return '';
        else if (this.status.NextExecutionScheduledAt) {
            const msUntil = new Date(this.status.NextExecutionScheduledAt).getTime() - new Date().getTime();
            const duration = DateUtils.prettifyDurationString(msUntil, '', 'now');
            return `Next run is scheduled to start at ${this.detailCodeTagStart}${this.formatDateTime(this.status.NextExecutionScheduledAt)}</code>, in ${this.detailCodeTagStart}${duration}</code>.`;
        }
        else return '';
    }
    
    // <li v-if="showLastResult">Last result was: <code class="inline-detail">{{ status.Summary }}</code></li>
    get statusText(): string {
        if (this.status.StartedAt != null && this.status.EndedAt != null && this.status.Summary != null) {
            return this.isJobRunning
                ? `Last reported status: ${this.detailCodeTagStart}${this.status.Summary}</code>`
                : `Last result was: ${this.detailCodeTagStart}${this.status.Summary}</code>`;
        }
        else return '';
    }

    get enabledStatusText(): string {
        if (!this.status.IsEnabled) return 'Job is currently disabled.';
        else return '';
    }

    get showLastResult(): boolean {
        return this.status.StartedAt != null && this.status.EndedAt != null && this.status.Summary != null;
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onPageIndexChanged(): void {
        this.loadPagedHistory(false);
    }
    onLogPageIndexChanged(): void {
        this.loadPagedLogItems(false);
    }

    @Watch("status", { deep: true })
    onStatusChanged(): void {
        this.isStarting = false;
        this.isStopping = false;
    }
}
</script>

<style scoped lang="scss">
.job-component {

}
.job__header {
    display: flex;
    align-items: center;
    h2 {
        margin-left: 5px;
    }
}
.job__status {
    font-size: 13px;
    color: var(--color--accent-darken10);
}
.history-entry,
.log-entry {
    padding: 5px;
    font-size: 12px;
    border-left: 4px solid transparent;
    &__header {
        display: flex;
        align-items: center;
    }
    &__summary {
        margin-left: 29px;
        margin-top: 2px;
    }
    &.clickable:hover {
        background-color: var(--color--info-lighten4);
        border-left: 4px solid var(--color--primary-base);
    }
    &:nth-child(odd) {
        background-color: var(--color--accent-base);
    }
    &:nth-child(even) {
        background-color: var(--color--accent-darken1);
    }
}
.job-parameter-item {
    margin-bottom: 10px;
}
.no-results-text {
    padding: 10px;
    font-weight: 600;
}
.editor {
  width: 100%;
  /* height: 100%; */
  height: 75vh;
}
</style>
