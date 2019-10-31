<!-- src/components/Pages/LogViewerPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
          
            <!-- FILTER -->
            <v-container grid-list-md>
                <h1 class="mb-4">Log Search</h1>

                <v-layout row wrap>
                    <v-flex xs12 sm12 md8 style="position:relative">
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
                            :disabled="logDataLoadInProgress"
                            timeFormat="HH:mm"
                            @onChange="onDateRangeChanged"
                        />
                    </v-flex>

                    <v-flex xs12 sm12 md4 style="text-align: right;">
                        <v-btn ripple color="error"
                            @click.stop.prevent="cancelSearch(currentSearchId)"
                            v-if="logDataLoadInProgress && currentSearchId != ''"
                            :disabled="cancellationInProgress">
                            <v-icon color="white">cancel</v-icon>
                            Cancel
                        </v-btn>
                        <v-btn @click="currentPage=1;loadData()" :disabled="logDataLoadInProgress" class="primary">Search</v-btn>
                        <v-btn @click="resetFilters" :disabled="logDataLoadInProgress">Reset</v-btn>
                    </v-flex>
                    
                    <!-- QUERY -->
                    <v-flex xs12>
                        <v-layout row xs12>
                            <v-flex xs10 sm10>
                                <v-text-field type="text" clearable
                                    label="Content filter"
                                    v-model="filterQuery"
                                    :disabled="logDataLoadInProgress"
                                    :error-messages="searchResultData.ParsedQuery.ParseError"
                                ></v-text-field>
                            </v-flex>
                            <v-flex xs2 sm2 class="pa-3">
                                <v-checkbox
                                    class="options-checkbox"
                                    v-model="filterQueryIsRegex"
                                    label="Regex"
                                ></v-checkbox>
                            </v-flex>
                        </v-layout>
                    </v-flex>
                    
                    <!-- EXCLUDED QUERY -->
                    <v-flex xs12 v-if="showExcludedQuery">
                        <v-layout row xs12>
                            <v-flex xs10 sm10>
                                <v-text-field type="text" clearable
                                    label="Content exclusion filter"
                                    v-model="filterExcludedQuery"
                                    :disabled="logDataLoadInProgress"
                                    :error-messages="searchResultData.ParsedExcludedQuery.ParseError"
                                ></v-text-field>
                            </v-flex>
                            <v-flex xs2 sm2 class="pa-3">
                                <v-checkbox
                                    class="options-checkbox"
                                    v-model="filterExcludedQueryIsRegex"
                                    label="Regex"
                                ></v-checkbox>
                            </v-flex>
                        </v-layout>
                    </v-flex>
                    
                    <!-- LOG PATH QUERY -->
                    <v-flex xs12 v-if="showLogPathQuery">
                        <v-layout row xs12>
                            <v-flex xs10 sm10>
                                <v-text-field type="text" clearable
                                    label="File path filter"
                                    v-model="filterLogPathQuery"
                                    :disabled="logDataLoadInProgress"
                                    :error-messages="searchResultData.ParsedLogPathQuery.ParseError"
                                ></v-text-field>
                            </v-flex>
                            <v-flex xs2 sm2 class="pa-3">
                                <v-checkbox
                                    class="options-checkbox"
                                    v-model="filterLogPathQueryIsRegex"
                                    label="Regex"
                                ></v-checkbox>
                            </v-flex>
                        </v-layout>
                    </v-flex>
                    
                    <!-- EXCLUDED LOG PATH QUERY -->
                    <v-flex xs12 v-if="showExcludedLogPathQuery">
                        <v-layout row xs12>
                            <v-flex xs10 sm10>
                                <v-text-field type="text" clearable
                                    label="File path exclusion filter"
                                    v-model="filterExcludedLogPathQuery"
                                    :disabled="logDataLoadInProgress"
                                    :error-messages="searchResultData.ParsedExcludedLogPathQuery.ParseError"
                                ></v-text-field>
                            </v-flex>
                            <v-flex xs2 sm2 class="pa-3">
                                <v-checkbox
                                    class="options-checkbox"
                                    v-model="filterExcludedLogPathQueryIsRegex"
                                    label="Regex"
                                ></v-checkbox>
                            </v-flex>
                        </v-layout>
                    </v-flex>
                    
                    <!-- CUSTOM COLUMNS -->
                    <v-flex xs12 v-if="showcustomColumnRule">
                        <v-layout row xs12>
                            <v-flex xs10 sm10>
                                <v-text-field type="text" clearable
                                    v-model="customColumnRule"
                                    :label="getDelimiterLabel()"
                                    :disabled="logDataLoadInProgress"
                                    :error-messages="getCustomColumnRuleError()"
                                    append-icon="keyboard_tab"
                                    @click:append="customColumnRule = (customColumnRule || '') +'\t'"
                                ></v-text-field>
                            </v-flex>
                            <v-flex xs2 sm2>
                                <v-select
                                    v-model="customColumnMode"
                                    :items="customColumnModeOptions"
                                    :disabled="logDataLoadInProgress"
                                    item-text="text" item-value="value" color="secondary">
                                </v-select>
                            </v-flex>
                        </v-layout>
                    </v-flex>
                </v-layout>

                <div>
                    <!-- Options -->
                    <v-layout row wrap xs12>
                        <v-flex xs6 sm4 lg2 v-if="!showExcludedQuery">
                            <v-btn depressed small class="extra-filter-btn"
                                @click="showExcludedQuery = true">
                                <v-icon >add</v-icon>
                                Exclude content
                            </v-btn>
                        </v-flex>
                        <v-flex xs6 sm4 lg2 v-if="!showLogPathQuery">
                            <v-btn depressed small class="extra-filter-btn"
                                @click="showLogPathQuery = true">
                                <v-icon >add</v-icon>
                                Filter log filepaths
                            </v-btn>
                        </v-flex>
                        <v-flex xs6 sm4 lg2 v-if="!showExcludedLogPathQuery">
                            <v-btn depressed small class="extra-filter-btn"
                                @click="showExcludedLogPathQuery = true">
                                <v-icon >add</v-icon>
                                Exclude log filepaths
                            </v-btn>
                        </v-flex>
                        <v-flex xs6 sm4 lg2 v-if="!showcustomColumnRule">
                            <v-btn depressed small class="extra-filter-btn"
                                @click="showcustomColumnRule = true; customColumnRule=options.DefaultColumnRule">
                                <v-icon >add</v-icon>
                                Custom columns
                            </v-btn>
                        </v-flex>
                            
                        <v-flex xs6 sm4 lg2 style="padding-left: 22px;">
                            <v-checkbox
                                class="options-checkbox"
                                v-model="expandAllRows"
                                :label="`Expand all rows`"
                            ></v-checkbox>
                        </v-flex>
                        <v-flex xs6 sm2 style="padding-left: 22px;">
                            <v-text-field type="number" label="Page size"
                                class="options-input"
                                v-model.number="filterTake" />
                        </v-flex>
                        <v-flex xs6 sm2 style="padding-left: 22px;">
                            <v-text-field type="number" label="Margin (ms)"
                                class="options-input"
                                v-model.number="filterMargin" />
                        </v-flex>
                    </v-layout>
                    
                </div>

                <!-- METADATA -->
                <div>
                    <v-chip v-if="hasSearched" class="mb-4">
                        Last searched used {{ prettifyDuration(searchResultData.DurationInMilliseconds) }}
                    </v-chip>
                    <v-chip v-if="searchResultData.WasCancelled" class="mb-4">
                        <b>Search was cancelled</b>
                    </v-chip>
                    <v-chip v-if="searchResultData.HighestDate != null" class="mb-4">
                        Total results: {{ searchResultData.TotalCount }}
                    </v-chip>
                    <v-chip v-if="searchResultData.LowestDate != null" class="mb-4">
                        First matching entry @ {{ formatDateForChip(new Date(searchResultData.LowestDate)) }}
                    </v-chip>
                    <v-chip v-if="searchResultData.HighestDate != null" class="mb-4">
                        Last matching entry @ {{ formatDateForChip(new Date(searchResultData.HighestDate)) }}
                    </v-chip>
                    <v-btn ripple color="error"
                        @click.stop.prevent="cancelAllSearches()"
                        v-if="options.CurrentlyRunningLogSearchCount > 0 && !hasCancelledAll"
                        :disabled="allCancellationInProgress"
                        class="mb-4">
                        <v-icon color="white">cancel</v-icon>
                        {{ cancelAllSearchesButtonText }}
                    </v-btn>
                </div>

                <!-- CHARTS -->
                <v-expansion-panel popout v-if="chartEntries.length > 0" class="mb-4">
                    <v-expansion-panel-content>
                        <template v-slot:header>
                            <div>{{insightsTitle}}</div>
                        </template>
                        <v-card>
                            <item-per-date-chart-component :entries="chartEntries" />
                            <!-- <v-card-text></v-card-text> -->
                        </v-card>
                    </v-expansion-panel-content>
                </v-expansion-panel>
            </v-container>

            <!-- PAGINATION -->
            <div class="text-xs-center mb-4" v-if="searchResultData.PageCount > 0">
                <v-pagination
                    v-model="currentPage"
                    :length="searchResultData.PageCount"
                    :disabled="logDataLoadInProgress"></v-pagination>
            </div>

            <!-- PROGRESS -->
            <v-progress-linear
              v-if="allCancellationInProgress || logDataLoadInProgress || cancellationInProgress"
              :indeterminate="true"
              height="4"
              class="mt-4"></v-progress-linear>

            <!-- ERRORS -->
            <v-alert
              :value="logDataLoadFailed"
              type="error">
              {{ logDataFailedErrorMessage }}
            </v-alert>

            <log-entry-table-component
                :entries="searchResultData.Items"
                :customColumnRule="sanitizedCustomColumnRule"
                :customColumnMode="customColumnMode"
                :expandAllRows="expandAllRows"
                />

            <!-- PAGINATION -->
            <div class="text-xs-center mb-4" v-if="searchResultData.PageCount > 0">
                <v-pagination
                    v-model="currentPage"
                    :length="searchResultData.PageCount"
                    :disabled="logDataLoadInProgress"></v-pagination>
            </div>

            <!-- PARSED QUERY -->
            <v-expansion-panel popout v-if="describedQuery.length > 0" class="mb-4">
                <v-expansion-panel-content>
                    <template v-slot:header>
                        <div>Parsed query</div>
                    </template>
                    <v-card>
                        <ul class="ma-4">
                            <li v-for="(description, index) in describedQuery"
                                :key="`filter-description-${index}`">
                                {{ description }}
                            </li>
                        </ul>
                    </v-card>
                </v-expansion-panel-content>
            </v-expansion-panel>

          <!-- CONTENT END -->
        </v-flex>
        </v-layout>
        </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import DateUtils from '../../util/DateUtils';
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
import LogEntryTableComponent from '../LogViewer/LogEntryTableComponent.vue';
import ItemPerDateChartComponent from '../LogViewer/ItemPerDateChartComponent.vue';
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import LogSearchFilter from '../../models/LogViewer/LogSearchFilter';
import LogSearchResult from '../../models/LogViewer/LogSearchResult';
import { FilterQueryMode } from '../../models/LogViewer/FilterQueryMode';
import { FilterDelimiterMode } from '../../models/LogViewer/FilterDelimiterMode';
import { ChartEntry } from '../LogViewer/ItemPerDateChartComponent.vue';
import * as XRegExp from 'xregexp';
import ParsedQuery from "../../models/LogViewer/ParsedQuery";

interface DatePickerPreset {
    name: string;
    from: Date;
    to: Date;
}

@Component({
    components: {
        DateTimePicker,
        LogEntryTableComponent,
        ItemPerDateChartComponent
    }
})
export default class LogViewerPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // Filter fields
    showExcludedQuery: boolean = false;
    showLogPathQuery: boolean = false;
    showExcludedLogPathQuery: boolean = false;
    showcustomColumnRule: boolean = false;
    currentSearchId: string = "";
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
    cancellationInProgress: boolean = false;
    allCancellationInProgress: boolean = false;
    hasCancelledAll: boolean = false;
    logDataLoadInProgress: boolean = false;
    logDataLoadFailed: boolean = false;
    logDataFailedErrorMessage: string = "";
    hasSearched: boolean = false;

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
            let needles = "'" + query.MustContain.joinForSentence("', '", "and") + "'";
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
                label: `${x.Timestamp.toLocaleString()}`
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
        const count = this.options.CurrentlyRunningLogSearchCount;
        const searchesWord = (count == 1) ? "search" : "searches";
        return `Cancel ${count} currently running ${searchesWord} (for all users)`;
    }

    get queryModeOptions(): any {
        return [
            { text: 'Exact', value: FilterQueryMode.Exact},
            { text: 'Any word', value: FilterQueryMode.AnyWord},
            { text: 'All words', value: FilterQueryMode.AllWords},
            { text: 'RegExp', value: FilterQueryMode.Regex}
        ];
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
    getQueryLabel(name: string, mode: FilterQueryMode, negate: boolean = false) {
        const negateText = negate ? 'not ' : '';
        if (mode == FilterQueryMode.Exact) {
            return `${name} must ${negateText}contain exact match`;
        } else if (mode == FilterQueryMode.AnyWord) {
            return `${name} must ${negateText}contain any of the words`;
        } else if (mode == FilterQueryMode.AllWords) {
            return `${name} must ${negateText}contain all of the words`;
        } else if (mode == FilterQueryMode.Regex) {
            return `${name} must ${negateText}match the regex pattern`;
        } else {
            console.log(name, mode, negate);
            return name;
        }
    }

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
        this.showcustomColumnRule = this.options.ApplyCustomColumnRuleByDefault;

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
        this.customColumnRule = (this.options.ApplyCustomColumnRuleByDefault)
            ? this.options.DefaultColumnRule : '';
        this.customColumnMode = (this.options.DefaultColumnModeIsRegex == true) 
            ? FilterDelimiterMode.Regex : FilterDelimiterMode.Delimiter;
    }

    cancelSearch(searchId: string): void {
        this.cancellationInProgress = true;

        this.sendPOSTRequest<boolean>(this.options.CancelLogSearchEndpoint, { searchId: searchId },
            (result: boolean) => {
                this.cancellationInProgress = false;
            },
            (error) => {
                this.cancellationInProgress = false;
            });
    }
    
    cancelAllSearches(): void {
        this.allCancellationInProgress = true;

        this.sendPOSTRequest<number>(this.options.CancelAllLogSearchesEndpoint, null,
            (count: number) => {
                console.log(`Cancelled searches: ${count}`);
                this.allCancellationInProgress = false;
                this.hasCancelledAll = true;
            },
            (error) => {
                this.allCancellationInProgress = false;
            });
    }
    
    sendPOSTRequest<TResponse>(
        url: string,
        payload: any,
        onDataRetrieved: (response: TResponse) => void,
        onError: (error: any) => void
    ) : void {
        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        fetch(`${url}${queryStringIfEnabled}`, {
            credentials: 'include',
            method: "POST",
            body: JSON.stringify(payload),
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => {
            try { 
                return response.json();
            } catch(e) {
                console.log(e);
                return response;
            }
        })
        .then((data: TResponse) => onDataRetrieved(data))
        .catch((e) => {
            onError(e);
            console.error(e);
        });
    }

    loadData(): void {
        this.logDataLoadInProgress = true;
        this.logDataLoadFailed = false;

        let payload = this.generateFilterPayload();
        this.currentSearchId = payload.SearchId || "";

        this.sendPOSTRequest(this.options.GetLogSearchResultsEndpoint, payload,
            this.onSearchResultRetrieved,
            (error) => {
                this.searchResultData = this.createEmptyResultData();
                this.logDataLoadInProgress = false;
                this.logDataLoadFailed = true;
                this.logDataFailedErrorMessage = `Failed to load data with the following error. ${error}.`;
            });
    }

    onSearchResultRetrieved(data: LogSearchResult): void {
        this.hasSearched = true;
        this.logDataLoadInProgress = false;

        data.Statistics = data.Statistics.map(x => {
            return {
                Timestamp: new Date(x.Timestamp),
                Severity: x.Severity
            }
        });

        this.searchResultData = data;
        this.currentPage = data.CurrentPage;
        
        this.logDataLoadFailed = data.HasError;
        this.logDataFailedErrorMessage = data.Error || "";
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
            MaxStatisticsCount: this.options.MaxInsightsEntryCount,
            // OrderDescending: boolean;
            
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
        } catch(e) {
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
        if (!this.logDataLoadInProgress) {
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

<style>
.extra-filter-chip .v-chip__content {
    cursor: pointer;
}
.extra-filter-chip .v-chip__content:hover {
    text-shadow: 0px 0px 0.5px #928484;
}
.options-checkbox.v-input {
    margin-top: 4px;
}
.options-input.v-input {
    margin-top: 0;
    padding-top: 4px;
}
</style>