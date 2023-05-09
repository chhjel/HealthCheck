<!-- src/components/modules/Metrics/MetricsPageComponent.vue -->
<template>
    <div>
        
        <div class="content-root">
            <h1>Metrics</h1>
            <p>Debug metrics to verify performance of code, values might be a bit delayed until the tracker is disposed.</p>

            <btn-component :disabled="loadStatus.inProgress" @click="loadData" class="mb-3 ml-0">
                <icon-component size="20px" class="mr-2">refresh</icon-component>
                Refresh
            </btn-component>
            
            <text-field-component v-model:value="filterText" label="Filter" class="mb-3" />

            <!-- LOAD PROGRESS -->
            <progress-linear-component
                v-if="loadStatus.inProgress"
                indeterminate color="success"></progress-linear-component>

            <!-- DATA LOAD ERROR -->
            <alert-component :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
            {{ loadStatus.errorMessage }}
            </alert-component>

            <div v-if="!hasData && !loadStatus.inProgress">
                <b>No metrics data was found.</b>
            </div>

            <div v-if="hasData" class="metrics">
                <div v-if="globalCounters.length > 0">
                    <h2>Global counters ({{ globalCounters.length }})</h2>
                    <ul>
                        <li v-for="(item, itemIndex) in globalCounters"
                            :key="`gcounter-${itemIndex}`">
                            <b class="mr-1">{{ item.key }}:</b> <code>{{ item.value.Value }}</code>
                            <br />
                            <small class="ml-1">Between {{ formatDate(item.value.FirstStored) }} and  {{ formatDate(item.value.LastChanged) }}</small>
                        </li>
                    </ul>
                </div>

                <div v-if="globalValues.length > 0" class="mt-4">
                    <h2>Global values ({{ globalValues.length }})</h2>
                    <ul v-if="globalValues.length > 0">
                        <li v-for="(item, itemIndex) in globalValues"
                            :key="`gvalue-${itemIndex}`">
                            <b class="mr-1">{{ item.key }}:</b> 
                            <span v-if="item.values.ValueCount == 1">
                                <code>{{ item.values.Average }}{{ item.values.Suffix }}</code>. 
                            </span>
                            <span v-if="item.values.ValueCount > 1">
                                <code>{{ item.values.Min }}{{ item.values.Suffix }}</code> to <code>{{ item.values.Max }}{{ item.values.Suffix }}</code>, average <code>{{ item.values.Average }}{{ item.values.Suffix }}</code>. 
                            </span>
                            n=<code>{{ item.values.ValueCount }}</code>
                            <br />
                            <span v-if="item.values.ValueCount == 1">
                                <small class="ml-1">{{ formatDate(item.values.LastChanged) }}</small>
                            </span>
                            <span v-if="item.values.ValueCount > 1">
                                <small class="ml-1">Between {{ formatDate(item.values.FirstStored) }} and {{ formatDate(item.values.LastChanged) }}</small>
                            </span>
                        </li>
                    </ul>
                </div>

                <div v-if="globalNotes.length > 0" class="mt-4">
                    <h2>Global notes ({{ globalNotes.length }})</h2>
                    <ul>
                        <li v-for="(item, itemIndex) in globalNotes"
                            :key="`gnote-${itemIndex}`">
                            <div style="display: flex; align-items: baseline;">
                                <b class="mr-1">{{ item.id }}</b>
                                <small class="ml-1"> ({{ formatDate(item.note.LastChanged) }}):</small>
                            </div>
                            <div><code>{{ item.note.Note }}</code></div>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import MetricsService from '@services/MetricsService';
import ModuleOptions from '@models/Common/ModuleOptions';
import ModuleConfig from '@models/Common/ModuleConfig';
import { GetMetricsViewModel } from "@generated/Models/Core/GetMetricsViewModel";
import LinqUtils from "@util/LinqUtils";

import { ModuleFrontendOptions } from '@components/modules/EndpointControl/EndpointControlPageComponent.vue.models';
import { StoreUtil } from "@util/StoreUtil";
@Options({
    components: {
        BlockComponent
    }
})
export default class MetricsPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<ModuleFrontendOptions>;

    service: MetricsService = new MetricsService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    datax: GetMetricsViewModel | null = null;

    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();
    filterText: string = '';

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get hasData(): boolean {
        return this.globalCounters.length > 0 
            || this.globalValues.length > 0
            || this.globalNotes.length > 0;
    }

    get globalCounters(): Array<any> {
        if (!this.datax || !this.datax.GlobalCounters) {
            return [];
        }

        return Object.keys(this.datax.GlobalCounters)
            .map(x => {
                return {
                    key: x,
                    value: this.datax!.GlobalCounters[x]
                };
            })
            .sort((a, b) => LinqUtils.SortBy(a, b, x => x.key))
            .filter(x => this.valueMatchesFilter(x.key));
    }

    get globalValues(): Array<any> {
        if (!this.datax || !this.datax.GlobalValues) {
            return [];
        }
        
        return Object.keys(this.datax.GlobalValues)
            .map(x => {
                return {
                    key: x,
                    values: this.datax!.GlobalValues[x]
                };
            })
            .sort((a, b) => LinqUtils.SortBy(a, b, x => x.key))
            .filter(x => this.valueMatchesFilter(x.key));
    }

    get globalNotes(): Array<any> {
        if (!this.datax || !this.datax.GlobalNotes) {
            return [];
        }
        
        return Object.keys(this.datax.GlobalNotes)
            .map(x => {
                return {
                    id: x,
                    note: this.datax!.GlobalNotes[x]
                };
            })
            .sort((a, b) => LinqUtils.SortBy(a, b, x => x.id))
            .filter(x => this.valueMatchesFilter(x.id) || this.valueMatchesFilter(x.note?.Note));
    }

    ////////////////
    //  METHODS  //
    //////////////
    valueMatchesFilter(value: string): boolean {
        if (this.filterText.trim().length == 0) return true;
        else return value?.toLowerCase()?.includes(this.filterText.toLowerCase());
    }

    loadData(): void {
        this.service.GetMetrics(this.loadStatus, { onSuccess: (data) => this.onDataRetrieved(data) });
    }

    onDataRetrieved(data: GetMetricsViewModel | null): void {
        this.datax = data;
    }

    formatDate(date: Date): string {
        return DateUtils.FormatDate(date, "dddd dd. MMMM yyyy HH:mm:ss");
    }
}
</script>

<style scoped lang="scss">
.metrics {
    overflow-x: auto;
    padding-bottom: 20px;
}
</style>
