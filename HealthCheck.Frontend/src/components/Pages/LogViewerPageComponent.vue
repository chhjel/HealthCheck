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

            <hr />
            Last searched used {{ prettifyDuration(searchResultData.DurationInMilliseconds) }}
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
    filterQuery: string = "";

    searchResultData: LogSearchResult = this.createEmptyResultData();
    logDataLoadInProgress: boolean = false;
    logDataLoadFailed: boolean = false;
    logDataFailedErrorMessage: string = "";

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        // this.resetFilters();
        // this.loadData();
    }

    ////////////////
    //  GETTERS  //
    //////////////
    // get AreaChoices(): Array<AuditEventArea | null> {
    //     let choices = new Array<AuditEventArea | null>();
    //     choices.push(null);
    //     for (let value in AuditEventArea) {
    //         choices.push(value as AuditEventArea);
    //     }
    //     return choices;
    // }

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.logDataLoadInProgress = true;
        this.logDataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetLogSearchResultsEndpoint}${queryStringIfEnabled}`;
        let payload = this.generateFilterPayload();
        fetch(url, {
            credentials: 'include',
            method: "POST",
            body: JSON.stringify(payload),
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        .then((data: LogSearchResult) => this.onSearchResultRetrieved(data))
        .catch((e) => {
            this.searchResultData = this.createEmptyResultData();
            this.logDataLoadInProgress = false;
            this.logDataLoadFailed = true;
            this.logDataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }

    createEmptyResultData(): LogSearchResult {
        return { TotalCount: 0, Count: 0, Items: [], ColumnNames: [], DurationInMilliseconds: 0 }
    } 

    generateFilterPayload(): Partial<LogSearchFilter> {
        return {
            Take: 50,
            Query: this.filterQuery
        };
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