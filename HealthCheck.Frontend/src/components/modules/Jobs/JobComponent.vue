<!-- src/components/modules/Jobs/JobsPageComponent.vue -->
<template>
    <div class="job-component">
        <hr />
        <code>job:{{job}}</code>
        <code>status:{{status}}</code>
        <hr />

        <div v-if="pagedHistory">
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
                <code
                    class="clickable"
                    @click="loadHistoryDetails(entry.DetailId)"
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
        <code>historyDetails:{{historyDetails}}</code>
        <hr />
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

@Options({
    components: {
        PagingComponent
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
    
    // Service
    service: JobsService = new JobsService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();

    id: string = IdUtils.generateId();
    pagedHistory: HCPagedJobHistoryEntryViewModel | null = null;
    historyDetails: HCJobHistoryDetailEntryViewModel | null = null;

    // Pagination
    totalHistoryCount: number = 0;
    pageSizeDefault: number = 4;
    pageIndex: number = 0;
    pageSize: number = this.pageSizeDefault;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        this.loadPagedHistory(true);
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadPagedHistory(resetPageIndex: boolean): void {
        if (resetPageIndex) {
            this.pageIndex = 0;
        }
        this.service.GetPagedHistory(this.job.SourceId, this.job.Definition.Id, this.pageIndex, this.pageSize, this.dataLoadStatus, {
            onSuccess: (data) => {
                this.pagedHistory = data;
                this.totalHistoryCount = data.TotalCount;
            }
        });
    }

    loadHistoryDetails(id: string): void {
        this.service.GetHistoryDetail(id, this.dataLoadStatus, {
            onSuccess: (data) => this.historyDetails = data
        });
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
</style>
