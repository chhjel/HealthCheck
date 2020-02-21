<!-- src/components/Pages/DataflowPageComponent.vue -->
<template>
    <div>
        <v-content>
            <!-- NAVIGATION DRAWER -->
            <v-navigation-drawer
                v-model="drawerState"
                clipped fixed floating app
                mobile-break-point="1000"
                dark
                class="menu testset-menu">

                <v-list expand class="menu-items">
                    <v-progress-linear 
                        v-if="metadataLoadInProgress"
                        indeterminate color="green"></v-progress-linear>
                    
                    <v-list-tile ripple
                        v-for="(stream, streamIndex) in streamMetadatas"
                        :key="`stream-menu-${streamIndex}`"
                        class="testset-menu-item"
                        :class="{ 'active': (selectedStream == stream) }"
                        @click="setActveStream(stream)"
                        :disabled="dataLoadInProgress">
                        <v-list-tile-title v-text="stream.Name"></v-list-tile-title>
                    </v-list-tile>
                </v-list>
            </v-navigation-drawer>
            
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        <v-container>
                            <!-- DATA LOAD ERROR -->
                            <v-alert :value="dataLoadFailed && selectedStream == null" type="error">
                            {{ dataFailedErrorMessage }}
                            </v-alert>

                            <!-- SELECTED DATAFLOW INFO -->
                            <v-layout v-if="selectedStream != null" style="flex-direction: column;">
                                <h3>{{ selectedStream.Name }}</h3>
                                <p>{{ selectedStream.Description }}</p>
                            </v-layout>
                            
                            <!-- NO DATAFLOW SELECTED INFO -->
                            <v-layout v-if="selectedStream == null && streamMetadatas.length > 0" style="flex-direction: column;">
                                <h3>No dataflow selected</h3>
                                <p>← Select one over there.</p>
                            </v-layout>

                            <!-- FILTERS -->
                            <div v-show="selectedStream != null">
                                <v-layout>
                                    <v-flex xs12 sm12 md8 style="position:relative"
                                        v-show="selectedStream != null && selectedStream.SupportsFilterByDate">
                                        <v-menu
                                            transition="slide-y-transition"
                                            bottom>
                                            <template v-slot:activator="{ on }">
                                                <v-btn flat icon color="primary" class="datepicker-button" v-on="on">
                                                    <v-icon>date_range</v-icon>
                                                </v-btn>
                                            </template>
                                            <v-list>
                                                <v-list-tile
                                                    v-for="(preset, i) in datePickerPresets"
                                                    :key="`datepicker-preset-${i}`"
                                                    @click="setDatePickerValue(preset)">
                                                    <v-list-tile-title>{{ preset.name }}</v-list-tile-title>
                                                </v-list-tile>
                                            </v-list>
                                        </v-menu>

                                        <date-time-picker
                                            ref="filterDate"
                                            :startDate="filterFromDate"
                                            :endDate="filterToDate"
                                            :singleDate="false"
                                            :disabled="dataLoadInProgress"
                                            timeFormat="HH:mm"
                                            @onChange="onDateRangeChanged"
                                        />
                                    </v-flex>
                                </v-layout>
                                
                                <v-layout>
                                    <v-flex xs6 sm2 style="margin-top: 22px;">
                                        <v-text-field type="number" label="Max entries to fetch"
                                            class="options-input"
                                            v-model.number="filterTake" />
                                    </v-flex>
                                    <v-flex xs6 sm2 style="margin-top: 17px; margin-left: 40px;">
                                        <v-btn 
                                            @click="loadStreamEntries()" 
                                            :disabled="dataLoadInProgress" 
                                            class="primary">Fetch data</v-btn>
                                    </v-flex>
                                    
                                </v-layout>

                                <v-select
                                    :items="filterChoices"
                                    item-text="text"
                                    item-value="value"
                                    label="Filter on property"
                                    v-model="selectedFilter"
                                    v-on:change="onFilterSelected"
                                ></v-select>
                                <div v-for="(filter, findex) in filters"
                                    :key="`dataflow-filter-${findex}`">
                                    <v-text-field
                                        v-model="filter.value"
                                        :label="filter.text"
                                        clearable
                                    ></v-text-field>
                                </div>
                            </div>

                            <!-- TABLE START -->
                            <v-data-table
                                v-if="selectedStream != null"
                                :headers="tableHeaders"
                                :items="streamEntries"
                                :loading="dataLoadInProgress"
                                :rows-per-page-items="tableRowsPerPageItems"
                                :pagination.sync="tablePagination"
                                :custom-sort="tableSorter"
                                item-key="Internal__Table__Id"
                                expand
                                class="elevation-1 audit-table">
                                <v-progress-linear v-slot:progress color="primary" indeterminate></v-progress-linear>
                                <template v-slot:no-data>
                                <v-alert :value="true" color="error" icon="warning" v-if="dataLoadFailed">
                                    {{ dataFailedErrorMessage }}
                                </v-alert>
                                </template>
                                <template v-slot:items="props">
                                    <tr
                                        @click="props.expanded = !props.expanded"
                                        class="audit-table-row">
                                        <td
                                            v-for="(col, colIndex) in getTableColumns(props.item, false)"
                                            :key="`dataflow-row-${props.index}-col-${colIndex}`">
                                            {{ col.value }}
                                        </td>
                                        <!-- <td width="200">{{ props }}</td> -->
                                        <!-- <td class="text-xs-left">{{ props }}</td> -->
                                    </tr>
                                </template>
                                <template v-slot:expand="props">
                                    <v-card flat>
                                        <v-card-text class="row-details">
                                            <div
                                                v-for="(col, colIndex) in getTableColumns(props.item, true)"
                                                :key="`dataflow-row-expanded-${props.index}-col-${colIndex}`"
                                                class="expanded-item-details">
                                                <dataflow-entry-property-value-component
                                                    :type="col.uiHint"
                                                    :raw="col.value"
                                                    :title="col.key" />
                                            </div>
                                        </v-card-text>
                                    </v-card>
                                </template>
                            </v-data-table>
                            <!-- TABLE END -->

                        </v-container>
                    </v-flex>
                </v-layout>
            </v-container>
          <!-- CONTENT END -->
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import LoggedEndpointDefinitionViewModel from '../../models/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from '../../models/RequestLog/LoggedEndpointRequestViewModel';
import { EntryState } from '../../models/RequestLog/EntryState';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import UrlUtils from "../../util/UrlUtils";
import KeyArray from "../../util/models/KeyArray";
import KeyValuePair from "../../models/Common/KeyValuePair";
import DataflowStreamMetadata from "../../models/Dataflow/DataflowStreamMetadata";
import DataflowEntry from "../../models/Dataflow/DataflowEntry";
import GetDataflowEntriesRequestModel from "../../models/Dataflow/GetDataflowEntriesRequestModel";
import { DataFlowPropertyUIHint, DataFlowPropertyUIVisibilityOption } from "../../models/Dataflow/DataFlowPropertyDisplayInfo";
import DataflowEntryPropertyValueComponent from '../Dataflow/EntryProperties/DataflowEntryPropertyValueComponent.vue';
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";

interface PropFilter
{
    propertyName: string;
    text: string;
    value: string;
}
interface StreamPropFilters {
   [key: string]: Array<PropFilter>;
} 

interface DatePickerPreset {
    name: string;
    from: Date;
    to: Date;
}

@Component({
    components: {
        DataflowEntryPropertyValueComponent,
        DateTimePicker
    }
})
export default class DataflowPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // UI STATE
    drawerState: boolean = true;
    metadataLoadInProgress: boolean = false;
    dataLoadInProgress: boolean = false;
    dataLoadFailed: boolean = true;
    dataFailedErrorMessage: string = '';

    streamMetadatas: Array<DataflowStreamMetadata> = [];
    selectedStream: DataflowStreamMetadata | null = null;
    streamEntries: Array<DataflowEntry> = [];

    filters: Array<PropFilter> = [
        {propertyName: "Code", text: "aaa", value: "1234" },
        { propertyName: "InsertionTime", text: "bbb", value: "17" }
    ];
    selectedFilter: string | null = null;
    filtersPerStream: StreamPropFilters = {};
    filterFromDate: Date = new Date();
    filterToDate: Date = new Date();
    filterTake: number = 50;

    // Table
    tableRowsPerPageItems: Array<any> 
        = [25, 50, 100, {"text":"$vuetify.dataIterator.rowsPerPageAll","value":-1}];
    tablePagination: any = {
        rowsPerPage: 25
    };

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.resetFilter();
        this.loadData();
    }

    created(): void {
        this.$parent.$parent.$on("onSideMenuToggleButtonClicked", this.toggleSideMenu);
    }

    beforeDestroy(): void {
      this.$parent.$parent.$off('onSideMenuToggleButtonClicked', this.toggleSideMenu);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get tableHeaders(): Array<any> {
        return this.getTableHeaders(false);
    }

    get filterChoices(): Array<any> {
        if (this.selectedStream == null)
        {
            return [];
        }
        
        return this.selectedStream.PropertyDisplayInfo
            .filter(x => 
                x.IsFilterable == true
                && x.Visibility != DataFlowPropertyUIVisibilityOption.Hidden
                && !this.filters.some(f => f.propertyName == x.PropertyName))
            .map(x => {
                return {
                    text: x.DisplayName || x.PropertyName,
                    value: x.PropertyName
                };
            });
    }

    get datePickerPresets(): Array<DatePickerPreset> {
        const endOfToday = new Date();
        endOfToday.setHours(23);
        endOfToday.setMinutes(59);

        return [
            { name: 'Last hour', from: DateUtils.CreateDateWithMinutesOffset(-60), to: endOfToday },
            { name: 'Today', from: DateUtils.CreateDateWithDayOffset(0), to: endOfToday },
            { name: 'Last 3 days', from: DateUtils.CreateDateWithDayOffset(-3), to: endOfToday },
            { name: 'Last 7 days', from: DateUtils.CreateDateWithDayOffset(-7), to: endOfToday },
            { name: 'Last 30 days', from: DateUtils.CreateDateWithDayOffset(-30), to: endOfToday },
            { name: 'Last 60 days', from: DateUtils.CreateDateWithDayOffset(-60), to: endOfToday },
            { name: 'Last 90 days', from: DateUtils.CreateDateWithDayOffset(-90), to: endOfToday }
        ];
    }

    ////////////////
    //  METHODS  //
    //////////////
    resetFilter(): void {
        this.filterFromDate = new Date();
        this.filterFromDate.setDate(this.filterFromDate.getDate() - 7);
        this.filterFromDate.setHours(0);
        this.filterFromDate.setMinutes(0);
        this.filterToDate = new Date();
        this.filterToDate.setHours(23);
        this.filterToDate.setMinutes(59);

        this.setDatePickerDate(this.filterFromDate, this.filterToDate);

        this.filterTake = 50;
        this.filters = [];
    }

    setDatePickerValue(preset: DatePickerPreset): void {
        this.setDatePickerDate(preset.from, preset.to);
    }

    setDatePickerDate(from: Date, to: Date): void {
        this.filterFromDate = from;
        this.filterToDate = to;

        let dateFilterFormat = 'yyyy MMM d  HH:mm';
        (<any>this.$refs.filterDate).selectDateString 
            = `${DateUtils.FormatDate(this.filterFromDate, dateFilterFormat)} - ${DateUtils.FormatDate(this.filterToDate, dateFilterFormat)}`;
    }

    loadData(): void {
        this.metadataLoadInProgress = true;
        this.dataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetDataflowStreamsMetadataEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "GET",
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        .then((diagramsData: Array<DataflowStreamMetadata>) => this.onDataFlowMetaDataRetrieved(diagramsData))
        .catch((e) => {
            this.metadataLoadInProgress = false;
            this.dataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onDataFlowMetaDataRetrieved(data: Array<DataflowStreamMetadata>): void {
        this.metadataLoadInProgress = false;
        this.streamMetadatas = data;
    }

    loadStreamEntries(): void {
        if (this.selectedStream == null)
        {
            return;
        }

        let fromDate = null;
        let toDate = null;
        if (this.selectedStream.SupportsFilterByDate)
        {
            fromDate = this.filterFromDate;
            toDate = this.filterToDate;
        }

        let propFilters: any = {};
        this.filters
            .filter(x => x.value != null && x.value.length > 0)
            .forEach(item => {
                propFilters[item.propertyName] = item.value;
            });

        let filter: GetDataflowEntriesRequestModel = {
            StreamId: this.selectedStream.Id,
            StreamFilter: {
                Take: this.filterTake,
                Skip: 0,
                FromDate: fromDate,
                ToDate: toDate,
                PropertyFilters: propFilters
            }
        };

        this.streamEntries = [];
        this.dataLoadInProgress = true;
        this.dataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetDataflowStreamEntriesEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "POST",
            body: JSON.stringify(filter),
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json'
            })
        })
        .then(response => response.json())
        .then((diagramsData: Array<DataflowEntry>) => this.onDataFlowDataRetrieved(diagramsData))
        .catch((e) => {
            this.dataLoadInProgress = false;
            this.dataLoadFailed = true;
            this.dataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onDataFlowDataRetrieved(data: Array<DataflowEntry>): void {
        this.dataLoadInProgress = false;
        let idCounter = 1;
        this.streamEntries = data.map(x => {
            let extra = {
                Internal__Table__Id: idCounter++
            };
            
            return { ...extra, ... x };
        });
    }

    onFilterSelected(): void {
        if (this.selectedFilter != null && this.filters.findIndex(x => x.propertyName == this.selectedFilter) == -1)
        {
            let text = this.selectedFilter;
            let info = null;
            if(this.selectedStream != null)
            {
                info = this.selectedStream.PropertyDisplayInfo.filter(x => x.PropertyName == this.selectedFilter)[0];
            }
            if (info != null)
            {
                text = info.DisplayName || info.PropertyName;
            }

            this.filters.push({
                propertyName: this.selectedFilter,
                text: text,
                value: ''
            });
        }

        this.$nextTick(() => {
            this.selectedFilter = '';
        });
    }

    setActveStream(stream: DataflowStreamMetadata): void {
        if (this.dataLoadInProgress) {
            return;
        }

        if (this.selectedStream != null)
        {
            this.filtersPerStream[this.selectedStream.Id] = this.filters;
        }

        this.selectedStream = stream;
        
        this.filters = this.filtersPerStream[this.selectedStream.Id] || [];
    }
    
    toggleSideMenu(): void {
        this.drawerState = !this.drawerState;
    }
    
    getTableHeaders(expanded: boolean): Array<any> {
        if (this.selectedStream == null)
        {
            return [];
        }

        let entry = this.streamEntries[0];
        if (entry == null) return [];

        let headers: Array<any> = [];
        for(let key in entry)
        {
            if (key == 'Internal__Table__Id')
            {
                continue;
            }

            headers.push({
                text: key,
                value: key,
                align: 'left',
                uiOrder: 99999999,
                visibility: DataFlowPropertyUIVisibilityOption.Always,
                type: DataFlowPropertyUIHint.Raw,
                isFilterable: false
            });
        }
        
        for(let info of this.selectedStream.PropertyDisplayInfo)
        {
            let header = headers.filter(x => x.value == info.PropertyName)[0];
            if (header == null) {
                header = {
                    align: 'left',
                };
                headers.push(header);
            }

            header.text = info.DisplayName || info.PropertyName;
            header.value = info.PropertyName,
            header.uiOrder = info.UIOrder,
            header.uiHint = info.UIHint || DataFlowPropertyUIHint.Raw,
            header.dateTimeFormat = info.DateTimeFormat,
            header.visibility = info.Visibility,
            header.isFilterable = info.IsFilterable
        }

        headers = headers
            .filter(x => x.visibility == DataFlowPropertyUIVisibilityOption.Always 
                      || (expanded && x.visibility == DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded)
                      || (!expanded && x.visibility == DataFlowPropertyUIVisibilityOption.OnlyInList))
            .sort((a, b) => LinqUtils.SortBy(a, b, (x) => x.uiOrder, true));

        return headers;
    }
    
    getTableColumns(entry: DataflowEntry, isExpanded: boolean): Array<any>
    {
        return this.getTableHeaders(isExpanded)
            .map(x => {
                return {
                    key: x.text,
                    value: this.getTableColumnValue((<any>entry)[x.value], x, isExpanded),
                    uiHint: x.uiHint
                };
            });
    }

    getTableColumnValue(raw: any, header: any, isExpanded: boolean): any
    {
        let uiHint = (header != null) ? header.uiHint : null;
        if (uiHint == DataFlowPropertyUIHint.DateTime)
        {
            return DateUtils.FormatDate(new Date(raw), header.dateTimeFormat);
        }
        else
        {
            return raw;
        }
    }

    tableSorter(items: DataflowEntry[], propertyName: string, isDescending: boolean): DataflowEntry[]
    {
        if (propertyName == null) {
            return items;
        }

        let header = this.tableHeaders.filter(x => x.value == propertyName)[0];
        let uiHint = (header != null) ? header.uiHint : null;

        if (uiHint === DataFlowPropertyUIHint.DateTime)
        {
            items = items.sort((a:DataflowEntry, b:DataflowEntry) => new Date((<any>b)[propertyName]).getTime() - new Date((<any>a)[propertyName]).getTime());
        }
        else {
            items = items.sort((a:DataflowEntry, b:DataflowEntry) => {
                var textA = (<any>a)[propertyName].toUpperCase();
                var textB = (<any>b)[propertyName].toUpperCase();
                return (textA < textB) ? -1 : (textA > textB) ? 1 : 0;
            });
        }
        
        if (isDescending === true) {
            items = items.reverse();
        }

        return items;
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDateRangeChanged(data: any): void {
        this.filterFromDate = data.startDate;
        this.filterToDate = data.endDate;
    }
}
</script>

<style scoped lang="scss">
.expanded-item-details
{
    padding: 5px;
    border-left: 4px solid #EEE;

    .expanded-item-details-title {
        font-weight: 600;
        display: inline-block;
    }
    .expanded-item-details-value {
        display: inline-block;
    }
}
.datepicker-button {
    float: right;
    position: absolute;
    right: 2px;
    top: 5px;
    z-index: 2;
}
.menu {
    box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
}
.filter {
    position: relative;
    margin-left: 44px;
    margin-top: 26px;
    margin-bottom: 18px;
    margin-right: 44px;
}
@media (max-width: 960px) {
    .menu-items { 
        margin-top: 67px;
    }
}
</style>