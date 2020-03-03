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

                <filterable-list-component 
                    :items="menuItems"
                    :groupByKey="`GroupName`"
                    :sortByKey="`GroupName`"
                    :filterKeys="[ 'Name', 'Description' ]"
                    :loading="metadataLoadInProgress"
                    :disabled="dataLoadInProgress"
                    ref="filterableList"
                    v-on:itemClicked="onMenuItemClicked"
                    />
            </v-navigation-drawer>
            
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        <v-container>
                            <!-- LOAD PROGRESS -->
                            <v-progress-linear 
                                v-if="dataLoadInProgress"
                                indeterminate color="green"></v-progress-linear>

                            <!-- DATA LOAD ERROR -->
                            <v-alert :value="dataLoadFailed" v-if="dataLoadFailed" type="error">
                            {{ dataFailedErrorMessage }}
                            </v-alert>

                            <!-- SELECTED DATAFLOW INFO -->
                            <v-layout v-if="selectedStream != null" style="flex-direction: column;">
                                <h3>{{ selectedStream.Name }}</h3>
                                <p v-html="selectedStream.Description"></p>
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
                                
                                <v-select
                                    :items="filterChoices"
                                    item-text="text"
                                    item-value="value"
                                    label="Filter on property"
                                    v-model="selectedFilter"
                                    v-on:change="onFilterSelected"
                                    v-show="filterChoices.length > 0"
                                ></v-select>
                                <div v-for="(filter, findex) in filters"
                                    :key="`dataflow-filter-${findex}`">
                                    <v-text-field
                                        v-model="filter.value"
                                        :label="filter.text"
                                        clearable
                                    ></v-text-field>
                                </div>

                                <v-layout>
                                    <v-flex xs6 sm2 style="margin-top: 22px;">
                                        <v-text-field type="number" label="Max items to fetch"
                                            class="options-input"
                                            v-model.number="filterTake" />
                                    </v-flex>
                                    <v-flex xs6 sm2 style="margin-top: 17px; margin-left: 40px;">
                                        <v-btn 
                                            @click="loadStreamEntries()" 
                                            :disabled="dataLoadInProgress" 
                                            class="primary">Fetch data</v-btn>
                                    </v-flex>
                                    <v-flex xs6 sm2 style="margin-top: 17px; margin-left: 25px;">
                                        <v-btn 
                                            @click="clearResults()" 
                                            :disabled="dataLoadInProgress"
                                            >Clear view</v-btn>
                                    </v-flex>
                                    
                                </v-layout>
                            </div>

                            <div v-if="selectedStream != null">
                                <!-- Results info -->
                                <v-layout style="flex-direction: column;">
                                    <i v-if="resultCount == 0 && streamsWithDataAttemptedLoadedAtLeastOnce.indexOf(selectedStream.Id) != -1">Could not find any matching items</i>
                                    <i v-if="resultCount > 0">Result count: {{ resultCount }}</i>
                                </v-layout>

                                <!-- TABLE START -->
                                <data-table-component
                                    v-if="resultCount > 0"
                                    :groups="streamEntryGroups"
                                    :headers="tableHeaders.map(x => x.text)"
                                    class="elevation-2">
                                    <template v-slot:cell="{ value }">
                                        <span v-if="value.uiHint=='HTML'" v-html="value.value"></span>
                                        <span v-else>{{ value.value }}</span>
                                    </template>
                                    <template v-slot:expandedItem="{ item }">
                                        <div
                                            v-for="(col, colIndex) in item.expandedValues"
                                            :key="`dataflow-row-expanded-${item.Internal__Table__Id}-col-${colIndex}`"
                                            class="expanded-item-details">
                                            <dataflow-entry-property-value-component
                                                :type="col.uiHint"
                                                :raw="col.value"
                                                :title="col.key" />
                                        </div>
                                    </template>
                                </data-table-component>
                                <!-- TABLE END -->
                            </div>

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
import FilterInputComponent from '.././Common/FilterInputComponent.vue';
import DataTableComponent, { DataTableGroup } from '.././Common/DataTableComponent.vue';
import FilterableListComponent, { FilterableListItem } from '.././Common/FilterableListComponent.vue';

interface PropFilter
{
    propertyName: string;
    text: string;
    value: string;
}
interface StreamPropFilters {
   [key: string]: Array<PropFilter>;
}
interface ResultCachePerStream {
   [key: string]: Array<DataTableGroup>;
}
interface DatePickerPreset {
    name: string;
    from: Date;
    to: Date;
}
interface DateRangeGroup {
    title: string;
    minDate: Date;
    maxDate: Date;
}
interface StreamGroup
{
    title: string;
    streams: Array<DataflowStreamMetadata>;
}

@Component({
    components: {
        DataflowEntryPropertyValueComponent,
        DateTimePicker,
        FilterInputComponent,
        DataTableComponent,
        FilterableListComponent
    }
})
export default class DataflowPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // UI STATE
    drawerState: boolean = true;
    streamsFilterText: string = "";
    metadataLoadInProgress: boolean = false;
    dataLoadInProgress: boolean = false;
    dataLoadFailed: boolean = false;
    dataFailedErrorMessage: string = '';

    streamGroups: Array<StreamGroup> = [];
    streamMetadatas: Array<DataflowStreamMetadata> = [];
    selectedStream: DataflowStreamMetadata | null = null;
    firstEntry: DataflowEntry | null = null;

    resultCache: ResultCachePerStream = {};
    streamEntryGroups: Array<DataTableGroup> = [];
    streamsWithDataAttemptedLoadedAtLeastOnce: Array<string> = [];

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
    get menuItems(): Array<FilterableListItem>
    {
        return this.streamMetadatas.map(x => {
            return {
                title: x.Name,
                subtitle: 'woot',
                data: x
            };
        });
    }

    get resultCount(): number {
        if (this.streamEntryGroups.length == 0) {
            return 0;
        } else {
            return this.streamEntryGroups.map(x => x.items.length).reduce((a, b) => a + b);
        }
    }

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

    get showFilterCounts(): boolean {
        return this.streamsFilterText.length > 0;
    }

    ////////////////
    //  METHODS  //
    //////////////
    // Invoked from parent
    public onPageShow(): void {
        const parts = (<any>window).dataflowState;
        if (parts != null && parts != undefined) {
            this.updateUrl(parts);
        }
    }

    setFromUrl(forcedParts: Array<string> | null = null): void {
        const parts = forcedParts || UrlUtils.GetHashParts();
        
        let didSelectStream = false;
        const selectedItem = parts[1];
        if (selectedItem !== undefined && selectedItem.length > 0) {
            let stream = this.streamMetadatas.filter(x => UrlUtils.EncodeHashPart(x.Name) == selectedItem)[0];
            if (stream != null)
            {
                didSelectStream = true;
                this.setActveStream(stream);
            }
        }

        if (!didSelectStream && this.streamMetadatas.length > 0)
        {
            this.setActveStream(this.streamMetadatas[0]);
        }
    }

    updateUrl(parts?: Array<string> | null): void {
        if (parts == null)
        {
            parts = ['dataflow'];

            if (this.selectedStream != null)
            {
                parts.push(UrlUtils.EncodeHashPart(this.selectedStream.Name));
            }
        }

        UrlUtils.SetHashParts(parts);
        
        // Some dirty technical debt before transitioning to propper routing :-)
        (<any>window).dataflowState = parts;
    }

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
            this.dataLoadFailed = true;
            this.metadataLoadInProgress = false;
            this.dataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    onDataFlowMetaDataRetrieved(data: Array<DataflowStreamMetadata>): void {
        this.metadataLoadInProgress = false;
        
        this.streamMetadatas = data.map(x => {
            x.GroupName = x.GroupName || 'Other';
            return x;
        });

        const originalUrlHashParts = UrlUtils.GetHashParts();
        this.setFromUrl(originalUrlHashParts);
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
        this.firstEntry = data[0];
        let idCounter = 1;

        if (this.selectedStream != null && this.streamsWithDataAttemptedLoadedAtLeastOnce.indexOf(this.selectedStream.Id) == -1)
        {
            this.streamsWithDataAttemptedLoadedAtLeastOnce.push(this.selectedStream.Id);
        }

        if (this.selectedStream != null
            && this.selectedStream.DateTimePropertyNameForUI != null
            && this.selectedStream.DateTimePropertyNameForUI.trim().length > 0)
        {
            let ranges = this.createDateRangeGroups(data);
            let propName: string = (this.selectedStream || { DateTimePropertyNameForUI: '' }).DateTimePropertyNameForUI || '';
            
            let groups: Array<DataTableGroup> = [];
            for (let i=0; i<ranges.length; i++)
            {
                let range = ranges[i];
                let itemsInGroup = data.filter(x => {
                    let time = new Date((<any>x)[propName]).getTime();
                    return time >= range.minDate.getTime() && time <= range.maxDate.getTime();
                });

                let group: DataTableGroup = { title: range.title, items: [] };
                group.items = itemsInGroup.map(x => {
                    return {
                        Internal__Table__Id: idCounter++,
                        values: this.getTableColumns(x, false),
                        expandedValues: this.getTableColumns(x, true)
                    };
                });
                groups.push(group);
            }

            this.streamEntryGroups = groups;
        }
        else
        {
            let group: DataTableGroup = { title: '', items: [] };
            group.items = data.map(x => {
                return {
                    Internal__Table__Id: idCounter++,
                    values: this.getTableColumns(x, false),
                    expandedValues: this.getTableColumns(x, true)
                };
            });
            this.streamEntryGroups = [ group ];
        }
    }

    clearResults(): void {
        if (this.selectedStream != null)
        {
            this.resultCache[this.selectedStream.Id] = [];
        }
        this.streamEntryGroups = [];
        this.resetFilter();
    }

    createDateRangeGroups(data: Array<DataflowEntry>): Array<DateRangeGroup> {
        if (this.selectedStream == null)
            throw Error('Selected stream is null');

        let propName: string = this.selectedStream.DateTimePropertyNameForUI || '';
        if (propName == null || propName.trim().length == 0)
            throw Error('DateTimePropertyNameForUI is null or empty');
        
        let dates = data.map(x => new Date((<any>x)[propName]).getTime());
        
        let minDate = new Date(Math.min(...dates));
        let maxDate = new Date(Math.max(...dates));
        
        let range = maxDate.getTime() - minDate.getTime();
        let seconds = range * 1000;
        let minutes = seconds * 60;
        let hours = minutes * 60;
        let days = hours * 24;

        let startOfToday = new Date();
        startOfToday.setHours(0);
        startOfToday.setMinutes(0);
        startOfToday.setMilliseconds(0);

        // if (days > 30)
        // {
        //     return [
        //         { title: 'Today', minDate: startOfToday, maxDate: maxDate },
        //         { title: 'Yesterday', minDate: minDate, maxDate: maxDate },
        //         { title: 'E', minDate: minDate, maxDate: maxDate }
        //     ];
        // }
        // else
        // {
        //     return [ { title: 'Last hours', minDate: minDate, maxDate: maxDate } ];
        // }
        return [ { title: '', minDate: minDate, maxDate: maxDate } ];
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
            this.resultCache[this.selectedStream.Id] = this.streamEntryGroups;
        }

        this.selectedStream = stream;
        (<FilterableListComponent>this.$refs.filterableList).setSelectedItem(stream);
        
        this.filters = this.filtersPerStream[this.selectedStream.Id] || [];
        this.streamEntryGroups = this.resultCache[this.selectedStream.Id] || [];
        this.updateUrl();
    }
    
    toggleSideMenu(): void {
        this.drawerState = !this.drawerState;
    }
    
    getTableHeaders(expanded: boolean): Array<any> {
        if (this.selectedStream == null)
        {
            return [];
        }

        let entry = this.firstEntry;
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
                    uiHint: x.uiHint || DataFlowPropertyUIHint.Raw
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
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMenuItemClicked(item: FilterableListItem): void {
        this.setActveStream(item.data);
    }

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
    padding-left: 10px;
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