<!-- src/components/modules/Jobs/JobsOverviewComponent.vue -->
<template>
    <div class="jobs-overview-component">
        <h2 class="mb-4">Job overview</h2>
        <div v-for="group in groupedjobDefinitions"
            :key="`group-${group.name}`"
            class="job-group">
            <div v-if="group.name"
                class="job-group__title">{{ group.name }}</div>
            <div v-for="job in group.jobs"
                :key="`job-${job.job.SourceId}-${job.job.Definition.Id}`"
                class="job">
                <div class="job__header">
                    <div class="job__icon" v-if="jobIcon(job)">
                        <icon-component :color="jobIconColor(job)">{{ jobIcon(job) }}</icon-component>
                    </div>
                    <a class="job__title" href="#"
                        @click.stop.prevent="gotoJob(job, false)"
                        @click.middle.stop.prevent="gotoJob(job, true)"
                        >{{ job.job.Definition.Name }}</a>
                </div>
                <div class="job__description">{{ job.job.Definition.Description }}</div>
                <div class="job__status" v-if="job.status">
                    <div class="job__status__time" v-if="createStatusTimestamp(job.status)">{{ createStatusTimestamp(job.status) }}</div>
                    <div class="job__status__data">{{ job.status.Summary }}</div>
                </div>
                <div class="job__history" v-if="job.latestHistory">
                    <div class="job__history__time" v-if="createHistoryTimestamp(job.latestHistory)">{{ createHistoryTimestamp(job.latestHistory) }}</div>
                    <div class="job__history__data">{{ job.latestHistory.Summary }}</div>
                </div>
            </div>
        </div>
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
import { HCJobStatusViewModel } from "@generated/Models/Core/HCJobStatusViewModel";
import { HCJobHistoryEntryViewModel } from "@generated/Models/Core/HCJobHistoryEntryViewModel";
import DateUtils from "@util/DateUtils";
import { HCJobHistoryStatus } from "@generated/Enums/Core/HCJobHistoryStatus";
import HashUtils from "@util/HashUtils";
import UrlUtils from "@util/UrlUtils";
import JobUtils from "./JobUtils";

interface DefGroup {
    name: string;
    jobs: Array<JobDetails>;
}
interface JobDetails {
    job: HCJobDefinitionWithSourceViewModel;
    status: HCJobStatusViewModel | null;
    latestHistory: HCJobHistoryEntryViewModel | null;
}

@Options({
    components: {
    }
})
export default class JobsOverviewComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    @Prop({ required: true })
    jobDefinitions: Array<HCJobDefinitionWithSourceViewModel>;

    @Prop({ required: true })
    jobStatuses: Array<HCJobStatusViewModel>;
    
    // Service
    service: JobsService = new JobsService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    latestHistoryPerJob: Array<HCJobHistoryEntryViewModel> = [];
    historyUpdateTimout: NodeJS.Timeout;
    autoRefresh: boolean = true;
    isMounted: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.isMounted = true;
        this.loadLatestHistoryPerJob();
    }

    beforeUnmount(): void {
        this.isMounted = false;
        if (this.historyUpdateTimout) {
            clearTimeout(this.historyUpdateTimout);
        }
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadLatestHistoryPerJob(): void {
        this.service.GetLatestHistoryPerJobId(this.dataLoadStatus, {
            onSuccess: (data) => {
                this.latestHistoryPerJob = data || [];

                if (this.autoRefresh && this.isMounted) {
                    this.historyUpdateTimout = setTimeout(this.loadLatestHistoryPerJob, 5000);
                }
            }
        });
    }

    getJobStatus(sourceId: string, jobId: string): HCJobStatusViewModel | null {
        return this.jobStatuses.find(x => x.SourceId == sourceId && x.JobId == jobId);
    }

    getJobHistory(sourceId: string, jobId: string): HCJobHistoryEntryViewModel | null {
        return this.latestHistoryPerJob.find(x => x.SourceId == sourceId && x.JobId == jobId);
    }

    createJobDetails(job: HCJobDefinitionWithSourceViewModel): JobDetails {
        return {
            job: job,
            status: this.getJobStatus(job.SourceId, job.Definition.Id),
            latestHistory: this.getJobHistory(job.SourceId, job.Definition.Id),
        };
    }

    createHistoryTimestamp(history: HCJobHistoryEntryViewModel): string {
        const timestamp = new Date(history.EndedAt);
        return DateUtils.FormatDate(timestamp, 'd. MMM HH:mm:ss');
    }

    createStatusTimestamp(status: HCJobStatusViewModel): string {
        let timestamp = null;
        const start = status.StartedAt == null ? null : new Date(status.StartedAt);
        const end = status.EndedAt == null ? null : new Date(status.EndedAt);
        if (start != null && end != null) timestamp = start.getTime() > end.getTime() ? start : end;
        else timestamp = end || start;

        return timestamp == null ? '' : DateUtils.FormatDate(timestamp, 'd. MMM HH:mm:ss');
    }

    jobIcon(job: JobDetails): string { return JobUtils.jobIcon(job.status, job.latestHistory); };
    jobIconColor(job: JobDetails): string { return JobUtils.jobIconColor(job.status, job.latestHistory); };

    gotoJob(job: JobDetails, newTab: boolean): void {
        if (newTab) {
            const idHash = this.hash(job.job.Definition.Id);
            const route = `#/jobs/${idHash}`;
            UrlUtils.openRouteInNewTab(route);
        } else {
            this.$emit('setActiveJobById', job.job.Definition.Id);
        }
    }

    hash(input: string) { return HashUtils.md5(input); }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get isLoading(): boolean {
        return this.dataLoadStatus.inProgress;
    }
    
    get groupedjobDefinitions(): Array<DefGroup> {
        const groups: Array<DefGroup> = [];
        const ungrouped = this.jobDefinitions.filter(x => !x.Definition.GroupName);
        if (ungrouped.length > 0) {
            groups.push({
                name: '',
                jobs: ungrouped.map(j => this.createJobDetails(j))
            });
        }

        this.jobDefinitions
            .filter(x => x.Definition.GroupName)
            .forEach(x => {
                let group = groups.find(g => g.name == x.Definition.GroupName);
                if (!group) {
                    group = { name: x.Definition.GroupName, jobs: [] };
                    groups.push(group);
                }
                group.jobs.push(this.createJobDetails(x));
            });
        return groups;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
}
</script>

<style scoped lang="scss">
.jobs-overview-component {

}
.job-group {
    margin-bottom: 20px;
    &__title {
        font-weight: 600;
        margin-bottom: 5px;
    }
}
.job {
    border-left: 2px solid var(--color--accent-base);
    padding-left: 10px;
    padding-top: 5px;
    padding-bottom: 5px;

    &__header {
        display: flex;
        align-items: center;
    }

    &__description,
    &__status,
    &__history {
        font-size: 12px;
        color: var(--color--accent-darken7);
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        margin-top: 1px;
        margin-left: 30px;
    }

    &__status,
    &__history {
        display: flex;
        font-size: 10px;

        &__time {
            font-weight: 600;
            margin-right: 5px;
        }
        &__data {

        }
    }

    &__icon {
        margin-right: 5px;
    }
}
</style>
