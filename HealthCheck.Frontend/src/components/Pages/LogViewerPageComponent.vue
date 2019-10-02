<!-- src/components/Pages/LogViewerPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
          
            <v-text-field
                v-model="filterQuery"
                append-outer-icon="search"
                box
                clear-icon="mdi-close-circle"
                clearable
                label="Search query"
                type="text"
                :loading="logDataLoadInProgress"
                :disabled="logDataLoadInProgress"
                @click:append-outer="loadData"
            ></v-text-field>

            <v-btn ripple color="error"
              @click.stop.prevent="cancelSearch(currentSearchId)"
              v-if="logDataLoadInProgress && currentSearchId != ''"
              :disabled="cancellationInProgress"
              class="ma-0 mr-2 mt-2 pl-1 pr-3 cancel-test-button">
              <v-icon color="white">cancel</v-icon>
              Cancel
            </v-btn>

            <v-btn ripple color="error"
              @click.stop.prevent="cancelAllSearches()"
              v-if="options.CurrentlyRunningLogSearchCount > 0 && !hasCancelledAll"
              :disabled="allCancellationInProgress"
              class="ma-0 mr-2 mt-2 pl-1 pr-3 cancel-test-button">
              <v-icon color="white">cancel</v-icon>
              {{ cancelAllSearchesButtonText }}
            </v-btn>

            <hr />
            Last searched used {{ prettifyDuration(searchResultData.DurationInMilliseconds) }}
            <div v-if="searchResultData.WasCancelled"><b>Search was cancelled</b></div>
            <hr />

            {{ searchResultData }}

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
// @ts-ignore
import { DateTimePicker } from "@lazy-copilot/datetimepicker";
import LogSearchFilter from '../../models/LogViewer/LogSearchFilter';
import LogSearchResult from '../../models/LogViewer/LogSearchResult';

@Component({
    components: {
        DateTimePicker
    }
})
export default class LogViewerPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // // Filter fields
    currentSearchId: string = "";
    filterQuery: string = "";

    searchResultData: LogSearchResult = this.createEmptyResultData();
    cancellationInProgress: boolean = false;
    allCancellationInProgress: boolean = false;
    hasCancelledAll: boolean = false;
    logDataLoadInProgress: boolean = false;
    logDataLoadFailed: boolean = false;
    logDataFailedErrorMessage: string = "";

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get cancelAllSearchesButtonText(): string {
        const count = this.options.CurrentlyRunningLogSearchCount;
        const searchesWord = (count == 1) ? "search" : "searches";
        return `Cancel ${count} currently running ${searchesWord} (for all users)`;
    }

    ////////////////
    //  METHODS  //
    //////////////
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
            Take: 50,
            Query: this.filterQuery
        };
    }

    generateSearchId(): string {
        return (<any>[1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, (c:any) =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }

    onSearchResultRetrieved(data: LogSearchResult): void {
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
}
</script>

<style scoped>
</style>

<style>
</style>