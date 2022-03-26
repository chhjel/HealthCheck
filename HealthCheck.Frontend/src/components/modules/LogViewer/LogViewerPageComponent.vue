<!-- src/components/modules/LogViewer/LogViewerPageComponent.vue -->
<template>
    <div>
        <div> <!-- PAGE-->
        <div fluid fill-height class="content-root">
        <div>
        <div class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
          
            <!-- FILTER -->
            <div grid-list-md>
                <h1 class="mb-4">Log Search</h1>

                <div row wrap>
                    <div xs12 sm12 md8 style="position:relative">
                        <menu-component
                            transition="slide-y-transition"
                            bottom>
                            <btn-component flat icon color="primary" class="datepicker-button">
                                <icon-component>date_range</icon-component>
                            </btn-component>
                            <div>
                                <div
                                    v-for="(preset, i) in datePickerPresets"
                                    :key="`datepicker-preset-${i}`"
                                    @click="setDatePickerValue(preset)">
                                    <div>{{ preset.name }}</div>
                                </div>
                            </div>
                        </menu-component>

                        <date-time-picker
                            ref="filterDate"
                            :startDate="filterFromDate"
                            :endDate="filterToDate"
                            :singleDate="false"
                            :disabled="searchLoadStatus.inProgress"
                            timeFormat="HH:mm"
                            @onChange="onDateRangeChanged"
                        />
                    </div>

                    <div xs12 sm12 md4 style="text-align: right;">
                        <btn-component ripple color="error"
                            @click.stop.prevent="cancelSearch(currentSearchId)"
                            v-if="searchLoadStatus.inProgress && currentSearchId != ''"
                            :disabled="cancelSearchStatus.inProgress">
                            <icon-component color="white">cancel</icon-component>
                            Cancel
                        </btn-component>
                        <btn-component @click="currentPage=1;loadData()" :disabled="searchLoadStatus.inProgress" class="primary">Search</btn-component>
                        <btn-component @click="resetFilters" :disabled="searchLoadStatus.inProgress">Reset</btn-component>
                    </div>
                    
                    <!-- QUERY -->
                    <div xs12>
                        <div row xs12>
                            <div xs10 sm10>
                                <text-field-component type="text" clearable
                                    label="Content filter"
                                    v-model:value="filterQuery"
                                    :disabled="searchLoadStatus.inProgress"
                                    :error-messages="searchResultData.ParsedQuery.ParseError"
                                ></text-field-component>
                            </div>
                            <div xs2 sm2 class="pa-3">
                                <checkbox-component
                                    class="options-checkbox"
                                    v-model:value="filterQueryIsRegex"
                                    label="Regex"
                                ></checkbox-component>
                            </div>
                        </div>
                    </div>
                    
                    <!-- EXCLUDED QUERY -->
                    <div xs12 v-if="showExcludedQuery">
                        <div row xs12>
                            <div xs10 sm10>
                                <text-field-component type="text" clearable
                                    label="Content exclusion filter"
                                    v-model:value="filterExcludedQuery"
                                    :disabled="searchLoadStatus.inProgress"
                                    :error-messages="searchResultData.ParsedExcludedQuery.ParseError"
                                ></text-field-component>
                            </div>
                            <div xs2 sm2 class="pa-3">
                                <checkbox-component
                                    class="options-checkbox"
                                    v-model:value="filterExcludedQueryIsRegex"
                                    label="Regex"
                                ></checkbox-component>
                            </div>
                        </div>
                    </div>
                    
                    <!-- LOG PATH QUERY -->
                    <div xs12 v-if="showLogPathQuery">
                        <div row xs12>
                            <div xs10 sm10>
                                <text-field-component type="text" clearable
                                    label="File path filter"
                                    v-model:value="filterLogPathQuery"
                                    :disabled="searchLoadStatus.inProgress"
                                    :error-messages="searchResultData.ParsedLogPathQuery.ParseError"
                                ></text-field-component>
                            </div>
                            <div xs2 sm2 class="pa-3">
                                <checkbox-component
                                    class="options-checkbox"
                                    v-model:value="filterLogPathQueryIsRegex"
                                    label="Regex"
                                ></checkbox-component>
                            </div>
                        </div>
                    </div>
                    
                    <!-- EXCLUDED LOG PATH QUERY -->
                    <div xs12 v-if="showExcludedLogPathQuery">
                        <div row xs12>
                            <div xs10 sm10>
                                <text-field-component type="text" clearable
                                    label="File path exclusion filter"
                                    v-model:value="filterExcludedLogPathQuery"
                                    :disabled="searchLoadStatus.inProgress"
                                    :error-messages="searchResultData.ParsedExcludedLogPathQuery.ParseError"
                                ></text-field-component>
                            </div>
                            <div xs2 sm2 class="pa-3">
                                <checkbox-component
                                    class="options-checkbox"
                                    v-model:value="filterExcludedLogPathQueryIsRegex"
                                    label="Regex"
                                ></checkbox-component>
                            </div>
                        </div>
                    </div>
                    
                    <!-- CUSTOM COLUMNS -->
                    <div xs12 v-if="showcustomColumnRule">
                        <div row xs12>
                            <div xs10 sm10>
                                <text-field-component type="text" clearable
                                    v-model:value="customColumnRule"
                                    :label="getDelimiterLabel()"
                                    :disabled="searchLoadStatus.inProgress"
                                    :error-messages="getCustomColumnRuleError()"
                                    append-icon="keyboard_tab"
                                    @click:append="customColumnRule = (customColumnRule || '') +'\t'"
                                ></text-field-component>
                            </div>
                            <div xs2 sm2>
                                <select-component
                                    v-model:value="customColumnMode"
                                    :items="customColumnModeOptions"
                                    :disabled="searchLoadStatus.inProgress"
                                    item-text="text" item-value="value" color="secondary">
                                </select-component>
                            </div>
                        </div>
                    </div>
                </div>

                <div>
                    <div row wrap xs12>
                        <div xs6 sm4 lg4 v-if="!showExcludedQuery">
                            <btn-component depressed small class="extra-filter-btn"
                                @click="showExcludedQuery = true">
                                <icon-component >add</icon-component>
                                Exclude content
                            </btn-component>
                        </div>
                        <div xs6 sm4 lg4 v-if="!showLogPathQuery">
                            <btn-component depressed small class="extra-filter-btn"
                                @click="showLogPathQuery = true">
                                <icon-component >add</icon-component>
                                Filter log filepaths
                            </btn-component>
                        </div>
                        <div xs12 sm4 lg4 v-if="!showExcludedLogPathQuery">
                            <btn-component depressed small class="extra-filter-btn"
                                @click="showExcludedLogPathQuery = true">
                                <icon-component >add</icon-component>
                                Exclude log filepaths
                            </btn-component>
                        </div>
                        <div xs6 sm2 style="padding-left: 22px;">
                            <text-field-component type="number" label="Page size"
                                class="options-input"
                                v-model.number="filterTake" />
                        </div>
                        <div xs6 sm2 style="padding-left: 22px;">
                            <text-field-component type="number" label="Margin (ms)"
                                class="options-input"
                                v-model.number="filterMargin" />
                        </div>          
                        <div xs6 sm4 lg2 style="padding-left: 22px;">
                            <checkbox-component
                                class="options-checkbox"
                                v-model:value="filterOrderDescending"
                                :label="`Newest first`"
                            ></checkbox-component>
                        </div>            
                        <div xs6 sm4 lg2 style="padding-left: 22px;">
                            <checkbox-component
                                class="options-checkbox"
                                v-model:value="expandAllRows"
                                :label="`Expand all rows`"
                            ></checkbox-component>
                        </div>
                        <div xs6 sm4 lg4 v-if="!showcustomColumnRule">
                            <btn-component depressed small class="extra-filter-btn"
                                @click="showcustomColumnRule = true; customColumnRule=options.Options.DefaultColumnRule">
                                <icon-component >add</icon-component>
                                Custom columns
                            </btn-component>
                        </div>
                    </div>
                    
                </div>

                <!-- METADATA -->
                <div>
                    <chip-component v-if="hasSearched" class="mb-4">
                        Last searched used {{ prettifyDuration(searchResultData.DurationInMilliseconds) }}
                    </chip-component>
                    <chip-component v-if="searchResultData.WasCancelled" class="mb-4">
                        <b>Search was cancelled</b>
                    </chip-component>
                    <chip-component v-if="searchResultData.HighestDate != null" class="mb-4">
                        Total results: {{ searchResultData.TotalCount }}
                    </chip-component>
                    <chip-component v-if="searchResultData.LowestDate != null" class="mb-4">
                        First matching entry @ {{ formatDateForChip(new Date(searchResultData.LowestDate)) }}
                    </chip-component>
                    <chip-component v-if="searchResultData.HighestDate != null" class="mb-4">
                        Last matching entry @ {{ formatDateForChip(new Date(searchResultData.HighestDate)) }}
                    </chip-component>
                    <btn-component ripple color="error"
                        @click.stop.prevent="cancelAllSearches()"
                        v-if="options.Options.CurrentlyRunningLogSearchCount > 0 && !hasCancelledAll"
                        :disabled="cancelAllSearchesStatus.inProgress"
                        class="mb-4">
                        <icon-component color="white">cancel</icon-component>
                        {{ cancelAllSearchesButtonText }}
                    </btn-component>
                </div>

                <!-- CHARTS -->
                <expansion-panel-component popout v-if="chartEntries.length > 0" class="mb-4">
                    <template #header><div>{{insightsTitle}}</div></template>
                    <template #content>
                        <div>
                            <item-per-date-chart-component :entries="chartEntries" />
                        </div>
                    </template>
                </expansion-panel-component>

                <!-- GROUPED ENTRIES //todo -->
                <expansion-panel-component popout v-if="Object.keys(searchResultData.GroupedEntries).length > 0" class="mb-4">
                    <template #header><div>Grouped entries (work in progress)</div></template>
                    <template #content>
                        <div>
                            <ul class="ma-4">
                                <li v-for="(groupKey, index) in Object.keys(searchResultData.GroupedEntries)"
                                    :key="`grouped-entries-${index}`">
                                    {{ groupKey }}: {{ searchResultData.GroupedEntries[groupKey].length }}
                                </li>
                            </ul>
                        </div>
                    </template>
                </expansion-panel-component>
            </div>

            <!-- PAGINATION -->
            <div class="text-xs-center mb-4" v-if="searchResultData.PageCount > 0">
                <pagination-component
                    v-model:value="currentPage"
                    :length="searchResultData.PageCount"
                    :disabled="searchLoadStatus.inProgress"></pagination-component>
            </div>

            <!-- PROGRESS -->
            <progress-linear-component
              v-if="cancelAllSearchesStatus.inProgress || searchLoadStatus.inProgress || cancelSearchStatus.inProgress"
              :indeterminate="true"
              height="4"
              class="mt-4"></progress-linear-component>

            <!-- ERRORS -->
            <alert-component
              :value="searchLoadStatus.failed"
              type="error">
              {{ searchLoadStatus.errorMessage }}
            </alert-component>

            <log-entry-table-component
                :entries="searchResultData.Items"
                :customColumnRule="sanitizedCustomColumnRule"
                :customColumnMode="customColumnMode"
                :expandAllRows="expandAllRows"
                />

            <!-- PAGINATION -->
            <div class="text-xs-center mb-4" v-if="searchResultData.PageCount > 0">
                <pagination-component
                    v-model:value="currentPage"
                    :length="searchResultData.PageCount"
                    :disabled="searchLoadStatus.inProgress"></pagination-component>
            </div>

            <!-- PARSED QUERY -->
            <expansion-panel-component popout v-if="describedQuery.length > 0" class="mb-4">
                <template #header><div>Parsed query</div></template>
                <template #content>
                    <div>
                        <ul class="ma-4">
                            <li v-for="(description, index) in describedQuery"
                                :key="`filter-description-${index}`">
                                {{ description }}
                            </li>
                        </ul>
                    </div>
                </template>
            </expansion-panel-component>

          <!-- CONTENT END -->
        </div>
        </div>
        </div>
        </div> <!-- /PAGE-->
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
import LogEntryTableComponent from '@components/modules/LogViewer/LogEntryTableComponent.vue';
import ItemPerDateChartComponent from '@components/modules/LogViewer/ItemPerDateChartComponent.vue';
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import { FilterDelimiterMode } from '@models/modules/LogViewer/FilterDelimiterMode';
import { ChartEntry } from '@components/Common/Charts/DataOverTimeChartComponent.vue.models';
import * as XRegExp from 'xregexp';
import LogService from '@services/LogService';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import LogSearchResult from "@models/modules/LogViewer/LogSearchResult";
import ParsedQuery from "@models/modules/LogViewer/ParsedQuery";
import LogSearchFilter from "@models/modules/LogViewer/LogSearchFilter";
import { StoreUtil } from "@util/StoreUtil";

interface DatePickerPreset {
    name: string;
    from: Date;
    to: Date;
}
interface LogViewerPageOptions
{
    CurrentlyRunningLogSearchCount: number;
    DefaultColumnRule: string;
    DefaultColumnModeIsRegex: boolean;
    ApplyCustomColumnRuleByDefault: boolean;
    MaxInsightsEntryCount: number;
}

@Options({
    components: {
        DateTimePicker,
        LogEntryTableComponent,
        ItemPerDateChartComponent
    }
})
export default class LogViewerPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<LogViewerPageOptions>;

    // Filter fields
    showExcludedQuery: boolean = false;
    showLogPathQuery: boolean = false;
    showExcludedLogPathQuery: boolean = false;
    showcustomColumnRule: boolean = false;
    currentSearchId: string = "";
    filterOrderDescending: boolean = false;
    filterFromDate: Date = new Date();
    filterToDate: Date = new Date();
    filterTake: number = 50;
    filterMargin: number = 0;
    filterQuery: string = "";
    filterQueryIsRegex: boolean = false;
    filterExcludedQuery: string = "";
    filterExcludedQueryIsRegex: boolean = false;
    filterLogPathQuery: string = "";
    filterLogPathQueryIsRegex: boolean = false;
    filterExcludedLogPathQuery: string = "";
    filterExcludedLogPathQueryIsRegex: boolean = false;

    customColumnMode: FilterDelimiterMode = FilterDelimiterMode.Regex;
    customColumnRule: string = "";
    expandAllRows: boolean = false;

    currentPage: number = 1;

    searchResultData: LogSearchResult = this.createEmptyResultData();
    hasCancelledAll: boolean = false;
    hasSearched: boolean = false;

    // Service
    service: LogService = new LogService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    searchLoadStatus: FetchStatus = new FetchStatus();
    cancelSearchStatus: FetchStatus = new FetchStatus();
    cancelAllSearchesStatus: FetchStatus = new FetchStatus();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.resetFilters();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }
    
    get describedQuery(): Array<string>
    {
        let parts: Array<Array<string>> = [
            this.describeParsedQuery('Result', this.searchResultData.ParsedQuery, false),
            this.describeParsedQuery('Result', this.searchResultData.ParsedExcludedQuery, true),
            this.describeParsedQuery('Log path', this.searchResultData.ParsedLogPathQuery, false),
            this.describeParsedQuery('Log path', this.searchResultData.ParsedExcludedLogPathQuery, true)
        ];

        return <any>parts
            .reduce((a, b) => [ ...a, ...b ], []);;
    }

    describeParsedQuery(name: string, query: ParsedQuery, negate: boolean): Array<string>
    {
        if (query.MustContain.length == 0 && query.MustContainOneOf.length == 0 && !query.IsRegex) {
            return [];
        }

        let parts = new Array<string>();
        let prefix = `${name} must `;
        if (negate) {
            prefix = `${prefix}not `;
        }

        if (query.MustContain.length > 0) {
            let needles = "'" + query.MustContain.joinForSentence("', '", "' and '") + "'";
            parts.push(`${prefix}contain ${needles}.`);
        }

        if (query.MustContainOneOf.length > 0) {
            let needles = "['" + query.MustContainOneOf.join("'], ['") + "']";
            if (query.MustContainOneOf.length == 1) {
                parts.push(`${prefix}contain at least one of ${needles}.`);
            } else {
                parts.push(`${prefix}contain at least one item from each of ${needles}.`);
            }
        }

        if (query.IsRegex) {
            parts.push(`${prefix}match the regex '${query.RegexPattern}'.`);
        }
        
        return parts;
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

    get chartEntries(): Array<ChartEntry> {
        return this.searchResultData.Statistics.map(x => {
            return {
                date: x.Timestamp,
                severity: x.Severity,
                label: `${x.Timestamp.toLocaleString()}`,
                group: ''
            };
        });
    }

    get insightsTitle(): string {
        if (!this.hasSearched || this.searchResultData.Statistics.length == 0) {
            return 'Statistics';
        }

        const dates = this.searchResultData.Statistics.map(x => x.Timestamp);
        const lowestDate = dates.reduce((a, b) => { return a < b ? a : b; }); 
        const highestDate = dates.reduce((a, b) => { return a > b ? a : b; });
        const dateRange = highestDate.getTime() - lowestDate.getTime();
        const from = this.getGroupedDate(lowestDate, dateRange);
        const to = this.getGroupedDate(highestDate, dateRange);

        const rangeDetails = `(${from} - ${to})`;
        return (this.searchResultData.StatisticsIsComplete)
            ? `Statistics for matching entries ${rangeDetails}` 
            : `Statistics for the first ${this.searchResultData.Statistics.length} matching entries ${rangeDetails}`;
    }

    getGroupedDate(date: Date, dateRange: number): string {
        if (dateRange > 7 * 24 * 60 * 60 * 1000) {
            return DateUtils.FormatDate(date, 'd. MMM. yyyy');
        }
        else if (dateRange > 8 * 60 * 60 * 1000) {
            return DateUtils.FormatDate(date, 'HH') + ':xx ' + DateUtils.FormatDate(date, 'd. MMM. yyyy');
        }
        else {
            return DateUtils.FormatDate(date, 'HH:mm') + ':xx ' + DateUtils.FormatDate(date, 'd. MMM. yyyy');
        }
    }

    get cancelAllSearchesButtonText(): string {
        const count = this.options.Options.CurrentlyRunningLogSearchCount;
        const searchesWord = (count == 1) ? "search" : "searches";
        return `Cancel ${count} currently running ${searchesWord} (for all users)`;
    }

    get customColumnModeOptions(): any {
        return [
            { text: 'RegExp', value: FilterDelimiterMode.Regex},
            { text: 'Delimiter', value: FilterDelimiterMode.Delimiter}
        ];
    }

    get sanitizedCustomColumnRule(): string {
        if (this.customColumnMode == FilterDelimiterMode.Regex && !this.isValidRegex(this.customColumnRule)) {
            return '';
        } else {
            return this.customColumnRule;
        }
    }

    ////////////////
    //  METHODS  //
    //////////////
    getDelimiterLabel(): string {
        return (this.customColumnMode == FilterDelimiterMode.Regex)
            ? "Create columns from RegExp group names"
            : "Create columns by splitting on string";
    }

    getCustomColumnRuleError(): string[] {
        if (this.customColumnMode == FilterDelimiterMode.Regex) {
            var error = this.getRegexError(this.customColumnRule);
            return (error == null) ? [] : [error];
        } else {
            return [];
        }
    }

    resetFilters(): void {
        this.showExcludedQuery = false;
        this.showLogPathQuery = false;
        this.showExcludedLogPathQuery = false;
        this.showcustomColumnRule = this.options.Options.ApplyCustomColumnRuleByDefault;

        this.filterFromDate = new Date();
        this.filterFromDate.setDate(this.filterFromDate.getDate() - 7);
        this.filterFromDate.setHours(0);
        this.filterFromDate.setMinutes(0);
        this.filterToDate = new Date();
        this.filterToDate.setHours(23);
        this.filterToDate.setMinutes(59);

        this.setDatePickerDate(this.filterFromDate, this.filterToDate);

        this.filterTake = 50;
        this.filterMargin = 0;
        this.filterQuery = "";
        this.filterExcludedQuery = "";
        this.filterLogPathQuery = "";
        this.filterExcludedLogPathQuery = "";
        this.filterQueryIsRegex = false;
        this.filterExcludedQueryIsRegex = false;
        this.filterLogPathQueryIsRegex = false;
        this.filterExcludedLogPathQueryIsRegex = false;
        this.customColumnRule = (this.options.Options.ApplyCustomColumnRuleByDefault)
            ? this.options.Options.DefaultColumnRule : '';
        this.customColumnMode = (this.options.Options.DefaultColumnModeIsRegex == true) 
            ? FilterDelimiterMode.Regex : FilterDelimiterMode.Delimiter;
    }

    cancelSearch(searchId: string): void {
        this.service.CancelSearch(searchId, this.cancelSearchStatus);
    }
    
    cancelAllSearches(): void {
        this.service.CancelAllSearches(this.cancelAllSearchesStatus, {
            onSuccess: (count) => {
                console.log(`Cancelled searches: ${count}`);
                this.hasCancelledAll = true;
            },
            onError: (err) => this.searchResultData = this.createEmptyResultData()
        })
    }

    loadData(): void {
        let payload = this.generateFilterPayload();
        this.currentSearchId = payload.SearchId || "";

        this.service.Search(payload, this.searchLoadStatus, {
            onSuccess: (data) => this.onSearchResultRetrieved(data),
            onError: (err) => this.searchResultData = this.createEmptyResultData()
        })
    }

    onSearchResultRetrieved(data: LogSearchResult): void {
        data.Statistics = data.Statistics.map(x => {
            return {
                Timestamp: new Date(x.Timestamp),
                Severity: x.Severity
            }
        });

        this.searchResultData = data;
        this.currentPage = data.CurrentPage;
        
        this.searchLoadStatus.failed = data.HasError;
        this.searchLoadStatus.errorMessage = data.Error || "";
    }

    createEmptyResultData(): LogSearchResult {
        return { 
            TotalCount: 0, Count: 0, Items: [], ColumnNames: [], DurationInMilliseconds: 0, 
            WasCancelled: false, Error: null, HasError: false, Statistics: [], HighestDate: null, LowestDate: null,
            StatisticsIsComplete: true, PageCount: 0, CurrentPage: 1,
            ParsedQuery: { MustContain: [], MustContainOneOf: [], IsRegex: false, RegexPattern: '' },
            ParsedLogPathQuery: { MustContain: [], MustContainOneOf: [], IsRegex: false, RegexPattern: '' },
            ParsedExcludedQuery: { MustContain: [], MustContainOneOf: [], IsRegex: false, RegexPattern: '' },
            ParsedExcludedLogPathQuery: { MustContain: [], MustContainOneOf: [], IsRegex: false, RegexPattern: '' },
            GroupedEntries: []
        };
    } 

    generateFilterPayload(): Partial<LogSearchFilter> {
        return {
            SearchId: this.generateSearchId(),
            Skip: (this.currentPage - 1) * this.filterTake,
            Take: this.filterTake,
            MarginMilliseconds: this.filterMargin,

            FromDate: this.filterFromDate,
            ToDate: this.filterToDate,
            MaxStatisticsCount: this.options.Options.MaxInsightsEntryCount,
            OrderDescending: this.filterOrderDescending,
            
            Query: this.filterQuery,
            QueryIsRegex: this.filterQueryIsRegex,
            ExcludedQuery: this.filterExcludedQuery,
            ExcludedQueryIsRegex: this.filterExcludedQueryIsRegex,
            LogPathQuery: this.filterLogPathQuery,
            LogPathQueryIsRegex: this.filterLogPathQueryIsRegex,
            ExcludedLogPathQuery: this.filterExcludedLogPathQuery,
            ExcludedLogPathQueryIsRegex: this.filterExcludedLogPathQueryIsRegex
        };
    }

    generateSearchId(): string {
        return (<any>[1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, (c:any) =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }
    
    prettifyDuration(milliseconds: number): string {
      if (milliseconds <= 0) {
        return "< 0ms";
      } else if(milliseconds > 1000) {
        let seconds = milliseconds / 1000;
        let multiplier = Math.pow(10, 2);
        seconds = Math.round(seconds * multiplier) / multiplier;
        return `${seconds}s`;
      } else {
        return `${milliseconds}ms`;
      }
    }

    isValidRegex(pattern: string): boolean {
        return this.getRegexError(pattern) == null;
    }

    getRegexError(pattern: string): string | null {
        try {
            XRegExp(pattern);
            return null;
        } catch(e: any) {
            return e.message || e; 
        }
    }

    formatDateForChip(date: Date): string {
        return DateUtils.FormatDate(date, 'd. MMM. HH:mm:ss yyyy');
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

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch("currentPage")
    onCurrentPageChanged(): void {
        if (!this.searchLoadStatus.inProgress) {
            this.loadData();
        }
    }

    onDateRangeChanged(data: any): void {
        this.filterFromDate = data.startDate;
        this.filterToDate = data.endDate;
    }
}
</script>

<style scoped>
.datepicker-button {
    float: right;
    position: absolute;
    right: 2px;
    top: 5px;
    z-index: 2;
}
</style>
