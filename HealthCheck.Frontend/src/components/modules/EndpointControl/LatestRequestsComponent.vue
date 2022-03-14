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

        <select-component style="display: inline-block" class="right"
            v-if="graphs && chartEntries.length > 0"
            v-model="barChartSortMode"
            :items="barChartSortOptions"
            @input="onBarChartSortModeChanged"
            />
        <v-btn :disabled="loadStatus.inProgress"
            @click="onRefreshClicked"
            class="mb-3">
            <v-icon size="20px" class="mr-2">refresh</v-icon>
            Refresh
        </v-btn>
        <div style="clear:both" class="mt-3"></div>

        <div v-if="graphs && (chartEntries.length > 0 || iPChartBars.length > 0 || chartBars.length > 0)">
            <bar-chart-component
                v-if="chartBars.length > 0"
                :sets="chartBarSets"
                :bars="chartBars"
                :title="`Latest ${simpleRequestHistory.length} requests per endpoint over the last ${timespanTextSinceEarliestSimpleData()}`"
                ylabel="Endpoints" />

            <bar-chart-component
                v-if="iPChartBars.length > 0"
                :sets="iPChartBarSets"
                :bars="iPChartBars"
                :title="`Top ${iPChartBars.length} IPs with the ${ipSortModeDescription} out of the last ${requestHistory.length} since ${timespanTextSinceEarliestFullData()} ago`"
                ylabel="IP" />

            <data-over-time-chart-component
                v-if="chartEntries.length > 0"
                :sets="chartSets"
                :entries="chartEntries"
                title="[count] total requests over the last [timespan]"
                ylabel="Requests"
                class="mt-2 mb-2" />
        </div>

        <div v-if="log && logEntries.length > 0">
            <h3>Latest {{ logEntries.length }} logged requests</h3>
            <paging-component
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
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import IdUtils from '@util/IdUtils';
import EndpointControlUtils from '@util/EndpointControl/EndpointControlUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import EndpointControlService from '@services/EndpointControlService';
import { EndpointControlCountOverDuration, EndpointControlEndpointDefinition, EndpointControlPropertyFilter, EndpointControlRule, EndpointRequestDetails, EndpointRequestSimpleDetails } from '@models/modules/EndpointControl/EndpointControlModels';
import DataOverTimeChartComponent from '@components/Common/Charts/DataOverTimeChartComponent.vue';
import { ChartEntry, ChartSet } from '@components/Common/Charts/DataOverTimeChartComponent.vue.models';
import BarChartComponent from '@components/Common/Charts/BarChartComponent.vue';
import { BarChartBar, BarChartSet } from '@components/Common/Charts/BarChartComponent.vue.models';
import PagingComponent from '@components/Common/Basic/PagingComponent.vue';
import SelectComponent from '@components/Common/Basic/SelectComponent.vue';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import { ModuleFrontendOptions } from '@components/modules/EndpointControl/EndpointControlPageComponent.vue.models';
import LinqUtils from '@util/LinqUtils';
import { Dictionary } from '@models/modules/EventNotifications/EventNotificationModels';
import { StoreUtil } from "@util/StoreUtil";

interface BarChartSortOption {
    text: string;
    value: BarChartSortMode;
}
enum BarChartSortMode {
    TotalRequestCount = 0,
    TotalBlockedCount,
    BlockedPercentage
}

@Options({
    components: {
        BlockComponent,
        DataOverTimeChartComponent,
        PagingComponent,
        BarChartComponent,
        SelectComponent
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
    requestHistory: Array<EndpointRequestDetails> = [];
    simpleRequestHistory: Array<EndpointRequestSimpleDetails> = [];
    barChartSortMode: BarChartSortMode = BarChartSortMode.BlockedPercentage;
    barChartSortOptions: Array<BarChartSortOption> = [
            { text: 'Total requests', value: BarChartSortMode.TotalRequestCount },
            { text: 'Blocked percentage', value: BarChartSortMode.BlockedPercentage },
            { text: 'Blocked count', value: BarChartSortMode.TotalBlockedCount }
        ];
    chartBars: Array<BarChartBar> = [];
    iPChartBars: Array<BarChartBar> = [];

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
        return StoreUtil.store.state.globalOptions;
    }

    get ipSortModeDescription(): string {
        if (this.barChartSortMode == BarChartSortMode.TotalBlockedCount)
        {
            return 'most blocked requests';
        }
        else if (this.barChartSortMode == BarChartSortMode.BlockedPercentage)
        {
            return 'highest percentage of blocked requests';
        }
        else
        {
            return 'most requests';
        }
    }

    get visibleLogEntries(): Array<EndpointRequestDetails> {
        const toSkip = (this.logEntriesPage - 1) * this.logEntriesPageSize;
        return this.logEntries
            // Skip(toSkip)
            .slice(toSkip, this.logEntries.length)
            // Take(logEntriesPageSize)
            .slice(0, this.logEntriesPageSize);
    }

    get iPChartBarSets(): Array<BarChartSet> {
        return [
            { label: 'Allowed', group: 'allowed', color: '#4cff50' },
            { label: 'Blocked', group: 'blocked', color: '#FF0000' }
        ];
    }

    updateIPChartBars(): void {
        let bars: Array<BarChartBar> = [];
        
        const allIps = this.requestHistory.map(x => x.UserLocationIdentifier);
        const distinctIps = allIps.filter((value, index) => allIps.indexOf(value) == index);
        let valuePerIp: Dictionary<number[]> = {};

        for (let item of this.requestHistory)
        {
            if (!valuePerIp[item.UserLocationIdentifier])
            {
                valuePerIp[item.UserLocationIdentifier] = [0, 0];
            }

            if (item.WasBlocked) {
                valuePerIp[item.UserLocationIdentifier][1]++;
            } else {
                valuePerIp[item.UserLocationIdentifier][0]++;
            }
        }

        for(let key in valuePerIp)
        {
            bars.push({
                label: EndpointControlUtils.getEndpointDisplayName(key, this.endpointDefinitions),
                values: valuePerIp[key]
            });
        }

        bars = this.sortBarChartData(bars).slice(0, 10);
        this.iPChartBars = bars;
    }

    get chartBarSets(): Array<BarChartSet> {
        return [
            { label: 'Allowed', group: 'allowed', color: '#4cff50' },
            { label: 'Blocked', group: 'blocked', color: '#FF0000' }
        ];
    }

    updateChartBars(): void {
        let bars = this.getChartBarsRequestsPerEndpoint();
        bars = this.sortBarChartData(bars);
        this.chartBars = bars;
    }

    sortBarChartData(bars: Array<BarChartBar>): Array<BarChartBar> {
        if (this.barChartSortMode == BarChartSortMode.TotalBlockedCount)
        {
            bars = bars.sort((a, b) => LinqUtils.SortBy(a, b, a => a.values[1]));
        }
        else if (this.barChartSortMode == BarChartSortMode.BlockedPercentage)
        {
            bars = bars.sort((a, b) => LinqUtils.SortBy(a, b, a => a.values[1] / (a.values[0] + a.values[1])));
        }
        else
        {
            bars = bars.sort((a, b) => LinqUtils.SortBy(a, b, a => a.values[0] + a.values[1]));
        }
        return bars;
    }

    getChartBarsRequestsPerEndpoint(): Array<BarChartBar> {
        let bars: Array<BarChartBar> = [];
        
        const allEndpointIds = this.simpleRequestHistory.map(x => x.EndpointId);
        const distinctEndpointIds = allEndpointIds.filter((value, index) => allEndpointIds.indexOf(value) == index);
        let valuesPerEndpointId: Dictionary<number[]> = {};

        for (let item of this.simpleRequestHistory)
        {
            if (!valuesPerEndpointId[item.EndpointId])
            {
                valuesPerEndpointId[item.EndpointId] = [0, 0];
            }

            if (item.WasBlocked)
            {
                valuesPerEndpointId[item.EndpointId][1]++;
            } else {
                valuesPerEndpointId[item.EndpointId][0]++;
            }
        }

        for(let key in valuesPerEndpointId)
        {
            bars.push({
                label: EndpointControlUtils.getEndpointDisplayName(key, this.endpointDefinitions),
                values: valuesPerEndpointId[key]
            });
        }

        return bars;
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
        this.requestHistory = data;
        this.logEntriesPage = 1;
        this.logEntries = data.sort((a,b) => LinqUtils.SortBy(a, b, a => new Date(a.Timestamp)));
        
        this.updateIPChartBars();
    }

    onSimpleDataRecieved(data: Array<EndpointRequestSimpleDetails>): void {
        this.simpleRequestHistory = data;
        this.chartEntries = [];
        for(let item of data)
        {
            this.chartEntries.push({
                date: new Date(item.Timestamp),
                group: item.WasBlocked ? 'blocked': 'allowed'
            });
        }

        this.updateChartBars();
    }

    formatDate(date: Date): string {
        return DateUtils.FormatDate(date, 'MMM d HH:mm:ss');
    }

    getEndpointName(endpointId: string): string {
        return EndpointControlUtils.getEndpointDisplayName(endpointId, this.endpointDefinitions);
    }

	timespanTextSinceEarliestFullData(): string {
        const dateRange = this.getDateRangeSinceEarliestFullData();
        return this.timespanTextFromRange(dateRange);
    }

	timespanTextSinceEarliestSimpleData(): string {
        const dateRange = this.getDateRangeSinceEarliestFullData();
        return this.timespanTextFromRange(dateRange);
    }

	timespanTextFromRange(dateRange: number): string {
		const minute = 60 * 1000;
		const hour = 60 * minute;
		const day = 24 * hour;

		const days = Math.floor(dateRange / day);
		const hours = Math.floor(dateRange / hour);
		const minutes = Math.floor(dateRange / minute);

        if (dateRange > (2 * day)) {
			return (days <= 1) ? `day` : `${days} days`;
        }
        else if (dateRange > (3 * hour)) {
			return (hours <= 1) ? `hour` : `${hours} hours`;
		}
		else {
			return (minutes <= 1) ? `minute` : `${minutes} minutes`;
		}
	}

    getDateRangeSinceEarliestFullData(): number {
		if (this.requestHistory.length <= 2)
		{
			return 0;
		}

        const dates = this.requestHistory.map(x => new Date(x.Timestamp));
        const min = dates.reduce((a, b) => { return a < b ? a : b; }); 
        const max = new Date();
        return max.getTime() - min.getTime();
    }

    getDateRangeSinceEarliestSimpleData(): number {
		if (this.simpleRequestHistory.length <= 2)
		{
			return 0;
		}

        const dates = this.simpleRequestHistory.map(x => new Date(x.Timestamp));
        const min = dates.reduce((a, b) => { return a < b ? a : b; }); 
        const max = new Date();
        return max.getTime() - min.getTime();
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onRefreshClicked(): void {
        this.refreshLatestRequests();
    }

    onBarChartSortModeChanged(): void {
        this.updateChartBars();
        this.updateIPChartBars();
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
