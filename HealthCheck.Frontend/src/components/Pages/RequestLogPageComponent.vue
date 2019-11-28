<!-- src/components/Pages/RequestLogPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
        <v-container fluid fill-height class="content-root">
        <v-layout>
        <v-flex class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            
        <v-container grid-list-md>
            <v-layout align-content-center wrap v-if="requestLogDataLoadInProgress || requestLogDataLoadFailed">
                <!-- LOAD ERROR -->
                <v-alert
                    :value="requestLogDataLoadFailed"
                    type="error">
                {{ requestLogDataFailedErrorMessage }}
                </v-alert>

                <!-- PROGRESS BAR -->
                <v-progress-linear
                    v-if="requestLogDataLoadInProgress"
                    indeterminate color="green"></v-progress-linear>
            </v-layout>

            <v-layout align-content-center wrap v-if="entries.length == 0 && !requestLogDataLoadInProgress && !requestLogDataLoadFailed">
                <v-alert type="info" :value="true">
                    No requests has been logged yet.
                </v-alert>
            </v-layout>

            <div v-if="entries.length > 0">
                <progress-bar-component class="progress" 
                    :max="progressBarMax" 
                    :success="progressBarSuccess" 
                    :error="progressBarError"
                    v-on:clickedSuccess="showOnlyState(STATE_SUCCESS)"
                    v-on:clickedError="showOnlyState(STATE_ERROR)"
                    v-on:clickedRemaining="showOnlyState(STATE_UNDETERMINED)" />
                <br />

                Order by:
                <div v-for="(sortOption, index) in sortOptions"
                     :key="`sortOption-${index}`">
                    <v-btn x-small @click="setSortOrder(sortOption)" >
                        {{ sortOption.name }}
                    </v-btn>
                </div>
                
                Show:
                <v-checkbox v-model="visibleStates" label="Successes" :value="STATE_SUCCESS"></v-checkbox>
                <v-checkbox v-model="visibleStates" label="Errors" :value="STATE_ERROR"></v-checkbox>
                <v-checkbox v-model="visibleStates" label="Undetermined" :value="STATE_UNDETERMINED"></v-checkbox>
                <br />

                <div v-for="(entry, index) in filteredEntries"
                    :key="`entry-${index}`">
                    <request-endpoint-component :options="options" :entry="entry" />
                </div>
            </div>
        </v-container>

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
import LoggedActionEntryViewModel from '../../models/RequestLog/LoggedActionEntryViewModel';
import LoggedActionCallEntryViewModel from '../../models/RequestLog/LoggedActionCallEntryViewModel';
import RequestEndpointComponent from '../RequestLog/RequestEndpointComponent.vue';
import ProgressBarComponent from '../Common/ProgressBarComponent.vue';
import { EntryState } from '../../models/RequestLog/EntryState';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import UrlUtils from "../../util/UrlUtils";

@Component({
    components: {
        ProgressBarComponent,
        RequestEndpointComponent
    }
})
export default class RequestLogPageComponent extends Vue {
    @Prop({ required: true })
    options!: FrontEndOptionsViewModel;

    // Loading
    requestLogDataLoadInProgress: boolean = false;
    requestLogDataLoadFailed: boolean = false;
    requestLogDataFailedErrorMessage: string = "";

    entries: Array<LoggedActionEntryViewModel> = [];
    sortOptions: Array<OrderByOption> = [
        {
            id: "successes",
            name: "Successes",
            sortedBy: (x: LoggedActionEntryViewModel) =>
                x.Errors.length > 0 ? -99999 : x.Calls.length,
            thenBy: null,
            state: EntryState.Success
        },
        {
            id: "errors",
            name: "Errors",
            sortedBy: (x: LoggedActionEntryViewModel) => x.Errors.length,
            thenBy: (x: LoggedActionEntryViewModel) => x.Calls.length,
            state: EntryState.Error
        },
        {
            id: "untested",
            name: "Untested",
            sortedBy: (x: LoggedActionEntryViewModel) => -(x.Calls.length + x.Errors.length),
            thenBy: (x: LoggedActionEntryViewModel) => x.Errors.length,
            state: EntryState.Undetermined
        }
    ];
    visibleStates: Array<EntryState> = [ EntryState.Success, EntryState.Error, EntryState.Undetermined ];
    currentlySortedBy: OrderByOption = this.sortOptions[0];
    sortedByDescending: boolean = true;
    sortedByThenDescending: boolean = true;
    STATE_SUCCESS: number = EntryState.Success;
    STATE_ERROR: number = EntryState.Error;
    STATE_UNDETERMINED: number = EntryState.Undetermined;

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
    get progressBarMax(): number {
        return this.entries.length;
    }
    get progressBarSuccess(): number {
        return this.entries.filter(x => x.State == EntryState.Success).length;
    }
    get progressBarError(): number {
        return this.entries.filter(x => x.State == EntryState.Error).length;
    }

    get filteredEntries(): Array<LoggedActionEntryViewModel> {
        return this.entries
            .filter(x => this.visibleStates.indexOf(x.State) !== -1)
            .sort((a, b) =>
                LinqUtils.SortByThenBy(a, b, this.currentlySortedBy.sortedBy, this.currentlySortedBy.thenBy));
    }

    ////////////////
    //  METHODS  //
    //////////////
    loadData(): void {
        this.requestLogDataLoadInProgress = true;
        this.requestLogDataLoadFailed = false;

        let queryStringIfEnabled = this.options.InludeQueryStringInApiCalls ? window.location.search : '';
        let url = `${this.options.GetRequestLogEndpoint}${queryStringIfEnabled}`;
        fetch(url, {
            credentials: 'include',
            method: "GET",
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
        .then(response => response.json())
        // .then(response => new Promise<Array<LoggedActionEntryViewModel>>(resolve => setTimeout(() => resolve(response), 3000)))
        .then((events: Array<LoggedActionEntryViewModel>) => this.onEventDataRetrieved(events))
        .catch((e) => {
            this.requestLogDataLoadInProgress = false;
            this.requestLogDataLoadFailed = true;
            this.requestLogDataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }
    
    onEventDataRetrieved(events: Array<LoggedActionEntryViewModel>): void {
        this.entries = events.map(x => {
            let state: EntryState =
                x.Errors.length > 0 ? EntryState.Error
                : x.Calls.length > 0 ? EntryState.Success : EntryState.Undetermined;
            
            return {
                ...x,
                State: state,
                Calls: x.Calls.map(c => {
                    return {
                        ...c,
                        Timestamp: new Date(c.Timestamp)
                    }
                }),
                Errors: x.Errors.map(c => {
                    return {
                        ...c,
                        Timestamp: new Date(c.Timestamp)
                    }
                })
            };
        });

        this.requestLogDataLoadInProgress = false;
    }
    
    showOnlyState(state: EntryState): void {
        this.visibleStates = [ state ];
        this.currentlySortedBy = this.sortOptions.find(x => x.state == state) || this.currentlySortedBy;
    }

    ensureStateIsVisible(state: EntryState): void {
        if (this.visibleStates.indexOf(state) === -1) {
            this.visibleStates.push(state);
        }
    }

    setSortOrder(order: OrderByOption): void {
        this.ensureStateIsVisible(order.state);
        this.currentlySortedBy = order;

        this.updateUrl();
    }

    setFromUrl(): void {
        const parts = UrlUtils.GetHashParts();
        
        const sortBy = this.sortOptions.find(x => x.id == parts[1]);
        if (sortBy !== undefined) {
            this.setSortOrder(sortBy);
        }

        const visible = parts[2];
        if (visible !== undefined) {
            this.visibleStates = visible.split('.').map(x => parseInt(x));
        }
    }

    updateUrl(): void {
        UrlUtils.SetHashParts([
            'requestlog',
            this.currentlySortedBy.id,
            this.visibleStates.join(".")
        ]);
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch("visibleStates")
    onVisibleStatesChanged(): void {
        this.updateUrl();
    }
}
type NumberGetter<T> = (a: T) => number;
interface OrderByOption {
    id: string;
    name: string;
    sortedBy: NumberGetter<LoggedActionEntryViewModel>;
    thenBy: NumberGetter<LoggedActionEntryViewModel> | null;
    state: EntryState;
}
</script>

<style scoped>
.progress {
    max-width: 1280px;
    margin: auto;
    margin-top: 40px;
    margin-bottom: 40px;
    cursor: pointer;
}
</style>

<style>
</style>