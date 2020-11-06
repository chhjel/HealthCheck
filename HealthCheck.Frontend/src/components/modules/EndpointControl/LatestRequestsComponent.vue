<!-- src/components/modules/EndpointControl/LatestRequestsComponent.vue -->
<template>
    <div class="root">
        <!-- DATA LOAD ERROR -->
        <v-alert :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
        {{ loadStatus.errorMessage }}
        </v-alert>
        
        <!-- LOAD PROGRESS -->
        <v-progress-linear
            v-if="loadStatus.inProgress"
            indeterminate color="green"></v-progress-linear>

        <v-btn :disabled="loadStatus.inProgress"
            @click="onRefreshClicked"
            class="mb-3">
            <v-icon size="20px" class="mr-2">refresh</v-icon>
            Refresh
        </v-btn>

        <div v-if="graphs && chartEntries.length > 0">
            <data-over-time-chart-component
                :sets="chartSets"
                :entries="chartEntries"
                title="[count] total requests over the last [timespan]"
                ylabel="Requests" />

            <div v-if="log && logEntries.length > 0">
                <h3>Latest {{ logEntries.length }} logged requests</h3>
                <simple-paging-component
                    :items="logEntries"
                    :pageSize="logEntriesPageSize"
                    v-model="logEntriesPage"
                    />
                <!-- <a @click="prevLogPage">&lt;&lt;</a>
                <a @click="nextLogPage">&gt;&gt;</a> -->
                <div v-for="(entry, eindex) in visibleLogEntries"
                    :key="`req-entry-${eindex}`"
                    class="req-log-entry"
                    :class="{ 'blocked': entry.WasBlocked, 'allowed': !entry.WasBlocked }">
                    <span>{{ formatDate(new Date(entry.Timestamp)) }}</span>
                    <span>{{ getEndpointName(entry.EndpointId) }}</span>
                    <span>{{ entry.WasBlocked ? 'Blocked' : 'Allowed' }}</span>
                    <span>{{ entry.UserLocationIdentifier }}</span>
                    <code v-if="entry.UserAgent != null && entry.UserAgent.length > 0">{{ entry.UserAgent }}</code>
                    <code v-if="entry.Url != null && entry.Url.length > 0">{{ entry.Url }}</code>
                    <!-- <a v-if="entry.BlockingRuleId != null">{{ entry.BlockingRuleId }}</a> -->
                </div>
            </div>
        </div>

    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import DateUtils from  '../../../util/DateUtils';
import IdUtils from  '../../../util/IdUtils';
import EndpointControlUtils from  '../../../util/EndpointControl/EndpointControlUtils';
import BlockComponent from  '../../Common/Basic/BlockComponent.vue';
import EndpointControlService from "../../../services/EndpointControlService";
import { EndpointControlCountOverDuration, EndpointControlEndpointDefinition, EndpointControlPropertyFilter, EndpointControlRule, EndpointRequestDetails, EndpointRequestSimpleDetails } from "../../../models/modules/EndpointControl/EndpointControlModels";
import DataOverTimeChartComponent, { ChartEntry, ChartSet } from '../../Common/Charts/DataOverTimeChartComponent.vue';
import SimplePagingComponent from '../../Common/Basic/SimplePagingComponent.vue';
import { FetchStatus } from "../../../services/abstractions/HCServiceBase";
import { ModuleFrontendOptions } from "./EndpointControlPageComponent.vue";
import LinqUtils from "../../../util/LinqUtils";

@Component({
    components: {
        BlockComponent,
        DataOverTimeChartComponent,
        SimplePagingComponent
    }
})
export default class LatestRequestsComponent extends Vue {
    @Prop({ required: true })
    moduleId!: string;

    @Prop({ required: false, default: null})
    endpointDefinitions!: Array<EndpointControlEndpointDefinition>;

    @Prop({ required: false, default: true})
    graphs!: boolean;

    @Prop({ required: false, default: true})
    log!: boolean;
    
    @Prop({ required: true })
    options!: ModuleFrontendOptions;

    service: EndpointControlService = new EndpointControlService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.moduleId);
    loadStatus: FetchStatus = new FetchStatus();

    logEntriesPage: number = 1;
    logEntriesPageSize: number = 100;
    logEntries: Array<EndpointRequestDetails> = [];
	chartSets: Array<ChartSet> = [
        { label: 'Allowed', group: 'allowed', color: '#4cff50' },
        { label: 'Blocked', group: 'blocked', color: '#FF0000' }
    ];
    chartEntries: Array<ChartEntry> = [];

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void {
        this.refreshLatestRequests();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }

    get visibleLogEntries(): Array<EndpointRequestDetails> {
        // logEntriesPage
        const toSkip = (this.logEntriesPage - 1) * this.logEntriesPageSize;
        return this.logEntries
            // Skip(toSkip)
            .slice(toSkip, this.logEntries.length)
            // Take(logEntriesPageSize)
            .slice(0, this.logEntriesPageSize);;
    }

    ////////////////
    //  METHODS  //
    //////////////
    refreshLatestRequests(): void {
        if (this.log)
        {
            this.service.GetLatestRequests(this.loadStatus, {
                onSuccess: (data) => this.onDataRecieved(data)
            });
        }

        if (this.graphs)
        {
            this.service.GetLatestRequestsSimple(this.loadStatus, {
                onSuccess: (data) => this.onSimpleDataRecieved(data)
            });
        }
    }

    onDataRecieved(data: Array<EndpointRequestDetails>): void {
        this.logEntriesPage = 1;
        this.logEntries = data.sort((a,b) => LinqUtils.SortBy(a, b, a => new Date(a.Timestamp)));
    }

    onSimpleDataRecieved(data: Array<EndpointRequestSimpleDetails>): void {
        this.chartEntries = [];
        for(let item of data)
        {
            this.chartEntries.push({
                date: new Date(item.Timestamp),
                group: item.WasBlocked ? 'blocked': 'allowed'
            });
        }
    }

    formatDate(date: Date): string {
        return DateUtils.FormatDate(date, 'MMM d HH:mm:ss');
    }

    getEndpointName(endpointId: string): string {
        return EndpointControlUtils.getEndpointDisplayName(endpointId, this.endpointDefinitions);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onRefreshClicked(): void {
        this.refreshLatestRequests();
    }
}
</script>

<style scoped lang="scss">
.req-log-entry {
    border-bottom: 1px solid #ccc;
    border-radius: 5px;
    padding: 10px;

    &.blocked {
        border-left: 3px solid var(--v-error-base);
    }

    &.allowed {
        border-left: 3px solid var(--v-success-base);

        code {
            color: #666;
        }
    }

    span {
        margin-right: 10px;
        padding-right: 10px;
        border-right: 2px solid #ccc;
    }
}
</style>
