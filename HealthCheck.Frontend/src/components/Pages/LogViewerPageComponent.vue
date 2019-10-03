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
                    <v-flex xs12 sm12 md8>
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
                        <v-btn @click="loadData" :disabled="logDataLoadInProgress" class="primary">Search</v-btn>
                        <v-btn @click="resetFilters" :disabled="logDataLoadInProgress">Reset</v-btn>
                    </v-flex>
                    
                    <!-- QUERY -->
                    <v-flex xs12>
                        <v-layout row xs12>
                            <v-flex xs10 sm10>
                                <v-text-field type="text" clearable
                                    v-model="filterQuery"
                                    :label="getQueryLabel('Log entry', filterQueryMode)"
                                    :disabled="logDataLoadInProgress"
                                ></v-text-field>
                            </v-flex>
                            <v-flex xs2 sm2>
                                <v-select
                                    v-model="filterQueryMode"
                                    :items="queryModeOptions"
                                    :disabled="logDataLoadInProgress"
                                    item-text="text" item-value="value" color="secondary">
                                </v-select>
                            </v-flex>
                        </v-layout>
                    </v-flex>
                    
                    <!-- EXCLUDED QUERY -->
                    <v-flex xs12 v-if="showExcludedQuery">
                        <v-layout row xs12>
                            <v-flex xs10 sm10>
                                <v-text-field type="text" clearable
                                    v-model="filterExcludedQuery"
                                    :label="getQueryLabel('Log entry', filterExcludedQueryMode, true)"
                                    :disabled="logDataLoadInProgress"
                                ></v-text-field>
                            </v-flex>
                            <v-flex xs2 sm2>
                                <v-select
                                    v-model="filterExcludedQueryMode"
                                    :items="queryModeOptions"
                                    :disabled="logDataLoadInProgress"
                                    item-text="text" item-value="value" color="secondary">
                                </v-select>
                            </v-flex>
                        </v-layout>
                    </v-flex>
                    
                    <!-- LOG PATH QUERY -->
                    <v-flex xs12 v-if="showLogPathQuery">
                        <v-layout row xs12>
                            <v-flex xs10 sm10>
                                <v-text-field type="text" clearable
                                    v-model="filterLogPathQuery"
                                    :label="getQueryLabel('Log file path', filterLogPathQueryMode)"
                                    :disabled="logDataLoadInProgress"
                                ></v-text-field>
                            </v-flex>
                            <v-flex xs2 sm2>
                                <v-select
                                    v-model="filterLogPathQueryMode"
                                    :items="queryModeOptions"
                                    :disabled="logDataLoadInProgress"
                                    item-text="text" item-value="value" color="secondary">
                                </v-select>
                            </v-flex>
                        </v-layout>
                    </v-flex>
                    
                    <!-- EXCLUDED LOG PATH QUERY -->
                    <v-flex xs12 v-if="showExcludedLogPathQuery">
                        <v-layout row xs12>
                            <v-flex xs10 sm10>
                                <v-text-field type="text" clearable
                                    v-model="filterExcludedLogPathQuery"
                                    :label="getQueryLabel('Log file path', filterExcludedLogPathQueryMode, true)"
                                    :disabled="logDataLoadInProgress"
                                ></v-text-field>
                            </v-flex>
                            <v-flex xs2 sm2>
                                <v-select
                                    v-model="filterExcludedLogPathQueryMode"
                                    :items="queryModeOptions"
                                    :disabled="logDataLoadInProgress"
                                    item-text="text" item-value="value" color="secondary">
                                </v-select>
                            </v-flex>
                        </v-layout>
                    </v-flex>
                </v-layout>

                <!-- Show extra filters -->
                <div>
                    <v-btn depressed small class="extra-filter-btn"
                        v-if="!showExcludedQuery" @click="showExcludedQuery = true">
                        <v-icon >add</v-icon>
                        Exclude content
                    </v-btn>
                    <v-btn depressed small class="extra-filter-btn"
                        v-if="!showLogPathQuery" @click="showLogPathQuery = true">
                        <v-icon >add</v-icon>
                        Limit log filepaths
                    </v-btn>
                    <v-btn depressed small class="extra-filter-btn"
                        v-if="!showExcludedLogPathQuery" @click="showExcludedLogPathQuery = true">
                        <v-icon >add</v-icon>
                        Exclude log filepaths
                    </v-btn>
                </div>

            </v-container>

            <!-- METADATA -->
            <div>
                <v-chip v-if="hasSearched" class="mb-4">
                    Last searched used {{ prettifyDuration(searchResultData.DurationInMilliseconds) }}
                </v-chip>
                <v-chip v-if="searchResultData.WasCancelled" class="mb-4">
                    <b>Search was cancelled</b>
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

            <!-- {{ searchResultData }} -->
            <div>
                <log-entry-component
                    v-for="(item, index) in searchResultData.Items"
                    :key="`log-entry-${index}`"
                    :entry="item"
                    />
            </div>


          <!-- CONTENT END -->
        </v-flex>
        </v-layout>
        </v-container>
        </v-content>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Page/FrontEndOptionsViewModel';
import DateUtils from '../../util/DateUtils';
import '@lazy-copilot/datetimepicker/dist/datetimepicker.css'
import LogEntryComponent from '../LogViewer/LogEntryComponent.vue';
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import LogSearchFilter from '../../models/LogViewer/LogSearchFilter';
import LogSearchResult from '../../models/LogViewer/LogSearchResult';
import { FilterQueryMode } from '../../models/LogViewer/FilterQueryMode';

@Component({
    components: {
        DateTimePicker,
        LogEntryComponent
    }
})
export default class LogViewerPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // Filter fields
    showExcludedQuery: boolean = false;
    showLogPathQuery: boolean = false;
    showExcludedLogPathQuery: boolean = false;
    currentSearchId: string = "";
    filterFromDate: Date = new Date();
    filterToDate: Date = new Date();
    filterSkip: number = 0;
    filterTake: number = 50;
    filterQuery: string = "";
    filterQueryMode: FilterQueryMode = FilterQueryMode.Exact;
    filterExcludedQuery: string = "";
    filterExcludedQueryMode: FilterQueryMode = FilterQueryMode.Exact;
    filterLogPathQuery: string = "";
    filterLogPathQueryMode: FilterQueryMode = FilterQueryMode.Exact;
    filterExcludedLogPathQuery: string = "";
    filterExcludedLogPathQueryMode: FilterQueryMode = FilterQueryMode.Exact;

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

    resetFilters(): void {
        this.showExcludedQuery = false;
        this.showLogPathQuery = false;
        this.showExcludedLogPathQuery = false;

        this.filterFromDate = new Date();
        this.filterFromDate.setDate(this.filterFromDate.getDate() - 365);
        this.filterFromDate.setHours(0);
        this.filterFromDate.setMinutes(0);
        this.filterToDate = new Date();
        this.filterToDate.setHours(23);
        this.filterToDate.setMinutes(59);

        this.filterSkip = 0;
        this.filterTake = 50;
        this.filterQuery = "";
        this.filterExcludedQuery = "";
        this.filterLogPathQuery = "";
        this.filterExcludedLogPathQuery = "";
        this.filterQueryMode = FilterQueryMode.Exact;
        this.filterExcludedQueryMode = FilterQueryMode.Exact;
        this.filterLogPathQueryMode = FilterQueryMode.Exact;
        this.filterExcludedLogPathQueryMode = FilterQueryMode.Exact;

        let dateFilterFormat = 'yyyy MMM d  HH:mm';
        (<any>this.$refs.filterDate).selectDateString 
            = `${DateUtils.FormatDate(this.filterFromDate, dateFilterFormat)} - ${DateUtils.FormatDate(this.filterToDate, dateFilterFormat)}`;
    }

    cancelSearch(searchId: string): void {
        this.cancellationInProgress = true;

        this.sendPOSTRequest<boolean>(this.options.CancelLogSearchEndpoint, { searchId: searchId },
            (result: boolean) => {
                console.log(`Was cancelled: ${result}`);
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
        .then(response => response.json())
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

    createEmptyResultData(): LogSearchResult {
        return { TotalCount: 0, Count: 0, Items: [], ColumnNames: [], DurationInMilliseconds: 0, WasCancelled: false }
    } 

    generateFilterPayload(): Partial<LogSearchFilter> {
        return {
            SearchId: this.generateSearchId(),
            Skip: this.filterSkip,
            Take: this.filterTake,

            FromDate: this.filterFromDate,
            ToDate: this.filterToDate,
            // OrderDescending: boolean;
            
            Query: this.filterQuery,
            QueryMode: this.filterQueryMode,
            ExcludedQuery: this.filterExcludedQuery,
            ExcludedQueryMode: this.filterExcludedQueryMode,
            LogPathQuery: this.filterLogPathQuery,
            LogPathQueryMode: this.filterLogPathQueryMode,
            ExcludedLogPathQuery: this.filterExcludedLogPathQuery,
            ExcludedLogPathQueryMode: this.filterExcludedLogPathQueryMode,

            // ColumnRegexPattern: string;
            // ColumnDelimiter: string;
        };
    }

    generateSearchId(): string {
        return (<any>[1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, (c:any) =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }

    onSearchResultRetrieved(data: LogSearchResult): void {
        this.hasSearched = true;
        this.logDataLoadInProgress = false;
        this.searchResultData = data;
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

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onDateRangeChanged(data: any): void {
        this.filterFromDate = data.startDate;
        this.filterToDate = data.endDate;
    }
}
</script>

<style scoped>
</style>

<style>
.extra-filter-chip .v-chip__content {
    cursor: pointer;
}
.extra-filter-chip .v-chip__content:hover {
    text-shadow: 0px 0px 0.5px #928484;
}
</style>