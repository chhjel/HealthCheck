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
                
                <div>
                States:
                    <v-checkbox v-model="visibleStates" label="Successes" :value="STATE_SUCCESS" style="display:inline-block"></v-checkbox>
                    <v-checkbox v-model="visibleStates" label="Errors" :value="STATE_ERROR" style="display:inline-block"></v-checkbox>
                    <v-checkbox v-model="visibleStates" label="Undetermined" :value="STATE_UNDETERMINED" style="display:inline-block"></v-checkbox>
                </div>
                <br />
               
                <div>
                    Verbs:
                    <v-checkbox
                        v-for="(verb, index) in verbs"
                        :key="`verb-${index}`"
                        v-model="visibleVerbs" :label="verb" :value="verb"
                        style="display:inline-block"></v-checkbox>
                </div>
                <br />

                <div>
                    Order by:
                    <v-btn x-small @click="setSortOrder(sortOption)"
                        v-for="(sortOption, index) in sortOptions"
                        :key="`sortOption-${index}`">
                        {{ sortOption.name }}
                    </v-btn>
                </div>
                <br />

                <v-checkbox v-model="groupEntries" label="Enable grouping" style="display:inline-block"></v-checkbox>
                <br />

                <!-- Versions:
                <div v-for="(version, index) in versions"
                     :key="`version-${index}`">
                    <v-checkbox v-model="visibleVersions" :label="version" :value="version"></v-checkbox>
                </div>
                <br /> -->

                <div v-if="groupEntries">
                    <div v-for="(group, index) in groupedFilteredEntries"
                        class="endpoint-group"
                        :key="`entry-group-${index}`">
                        <h3 class="endpoint-group-header">{{ group.Key }}</h3>
                        <div v-for="(subgroup, subindex) in group.Value"
                            class="endpoint-subgroup"
                            :key="`entry-${index}-subgroup-${subindex}`">
                            <h4 class="endpoint-subgroup-header">{{ subgroup.Key }}</h4>
                            <request-endpoint-component
                                v-for="(entry, index) in subgroup.Value"
                                class="endpoint"
                                :key="`entry-${index}-${subindex}`"
                                :options="options"
                                :entry="entry"
                                />
                        </div>
                    </div>
                </div>

                <div v-if="!groupEntries">
                    <request-endpoint-component
                        v-for="(entry, index) in filteredEntries"
                        :key="`entry-${index}`"
                        class="endpoint"
                        :options="options"
                        :entry="entry"
                        />
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
import KeyArray from "../../util/models/KeyArray";
import KeyValuePair from "../../models/Common/KeyValuePair";

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
    verbs: Array<string> = [];
    versions: Array<string> = [];
    sortOptions: Array<OrderByOption> = [
        {
            id: "successes-first",
            name: "Successes",
            sortedBy: (x: LoggedActionEntryViewModel) =>
                x.Errors.length > 0 ? -99999 : x.Calls.length,
            thenBy: null,
            state: EntryState.Success
        },
        {
            id: "errors-first",
            name: "Errors",
            sortedBy: (x: LoggedActionEntryViewModel) => x.Errors.length,
            thenBy: (x: LoggedActionEntryViewModel) => x.Calls.length,
            state: EntryState.Error
        },
        {
            id: "untested-first",
            name: "Untested",
            sortedBy: (x: LoggedActionEntryViewModel) => -(x.Calls.length + x.Errors.length),
            thenBy: (x: LoggedActionEntryViewModel) => x.Errors.length,
            state: EntryState.Undetermined
        },
        {
            id: "latest-first",
            name: "Latest calls",
            sortedBy: (x: LoggedActionEntryViewModel) => Math.max(
                ...x.Calls.map(c => c.Timestamp.getTime()).concat(x.Errors.map(c => c.Timestamp.getTime()))
            ),
            thenBy: null,
            state: null
        }
    ];
    visibleStates: Array<EntryState> = [ EntryState.Success, EntryState.Error, EntryState.Undetermined ];
    visibleVerbs: Array<string> = [];
    currentlySortedBy: OrderByOption = this.sortOptions[0];
    sortedByDescending: boolean = true;
    sortedByThenDescending: boolean = true;
    groupEntries: boolean = true;
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
        return this.entries
            .filter(x => this.includeEntryInProgress(x))
            .length;
    }
    get progressBarSuccess(): number {
        return this.entries
            .filter(x => this.includeEntryInProgress(x))
            .filter(x => x.State == EntryState.Success).length;
    }
    get progressBarError(): number {
        return this.entries
            .filter(x => this.includeEntryInProgress(x))
            .filter(x => x.State == EntryState.Error).length;
    }

    get filteredEntries(): Array<LoggedActionEntryViewModel> {
        return this.entries
            .filter(x => this.visibleStates.indexOf(x.State) !== -1)
            .filter(x => this.visibleVerbs.indexOf(x.HttpVerb) !== -1)
            // .filter(x => x.Calls.some(c => this.visibleVersions.indexOf(c.Version) !== -1))
            .sort((a, b) =>
                LinqUtils.SortByThenBy(a, b, this.currentlySortedBy.sortedBy, this.currentlySortedBy.thenBy));
    }

    get groupedFilteredEntries(): Array<EntryGroupGroup> {
        let groups: Array<EntryGroup> = LinqUtils.GroupByIntoKVP(
            this.filteredEntries,
            x => x.Group || "No group"
        );

        let groupGroups = new Array<EntryGroupGroup>();
        groups.forEach(group => {
            let subgroups = LinqUtils.GroupByIntoKVP(
                group.Value,
                x => x.Controller
            );
            groupGroups.push({
                Key: group.Key,
                Value: subgroups
            })
        });
        return groupGroups;
    }

    ////////////////
    //  METHODS  //
    //////////////
    includeEntryInProgress(entry: LoggedActionEntryViewModel): boolean {
        return true; //entry.Calls.length == 0 || entry.Calls.some(c => this.visibleVersions.indexOf(c.Version) !== -1);
    }

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
        const originalUrlHashParts = UrlUtils.GetHashParts();

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

        this.versions = this.entries.length == 0 ? [] : Array.from(new Set(
            this.entries
                .map(x => x.Calls.map(c => c.Version))
                .reduce((prev, cur) => prev.concat(cur))
        ));
        // this.visibleVersions = this.versions;

        this.verbs = this.entries.length == 0 ? [] : Array.from(new Set(this.entries.map(x => x.HttpVerb)));
        this.verbs.forEach(verb => this.visibleVerbs.push(verb));

        this.setFromUrl(originalUrlHashParts);
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
        if (order.state != null)
        {
            this.ensureStateIsVisible(order.state);
        }
        this.currentlySortedBy = order;

        this.updateUrl();
    }

    setFromUrl(forcedParts: Array<string> | null = null): void {
        const parts = forcedParts || UrlUtils.GetHashParts();
        
        const sortBy = this.sortOptions.find(x => x.id == parts[1]);
        if (sortBy !== undefined) {
            this.setSortOrder(sortBy);
        }

        const visible = parts[2];
        if (visible !== undefined && visible !== '.') {
            this.visibleStates = visible.split('.').filter(x => x.length > 0).map(x => parseInt(x));
        }

        const grouped = parts[3];
        if (grouped !== undefined && grouped !== '.') {
            this.groupEntries = grouped === '0';
        }

        const verbs = parts[4];
        if (verbs !== undefined && verbs !== '.') {
            this.visibleVerbs = verbs.split('.').map(x => x.toUpperCase());
        }
    }

    updateUrl(parts?: Array<string> | null): void {
        parts = parts || [
            'requestlog',
            (this.currentlySortedBy.id == this.sortOptions[0].id)
                ? '.' : this.currentlySortedBy.id,
            (this.visibleStates.length == 3)
                ? '.' : this.visibleStates.join("."),
            this.groupEntries ? '.' : '1',
            (this.visibleVerbs.length == this.verbs.length)
                ? '.' : this.visibleVerbs.map(x => x.toLowerCase()).join('.')
        ];

        UrlUtils.SetHashParts(parts);
        
        // Some dirty technical debt before transitioning to propper routing :-)
        (<any>window).requestLogState = parts;
    }

    // Invoked from parent
    public onPageShow(): void {
        const parts = (<any>window).requestLogState;
        if (parts != null && parts != undefined) {
            this.updateUrl(parts);
        }
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    @Watch("visibleStates")
    onVisibleStatesChanged(): void {
        this.updateUrl();
    }

    @Watch("groupEntries")
    onGroupEntriesChanged(): void {
        this.updateUrl();
    }

    @Watch("visibleVerbs")
    onVisibleVerbsChanged(): void {
        this.updateUrl();
    }
}
type NumberGetter<T> = (a: T) => number;
interface OrderByOption {
    id: string;
    name: string;
    sortedBy: NumberGetter<LoggedActionEntryViewModel>;
    thenBy: NumberGetter<LoggedActionEntryViewModel> | null;
    state: EntryState | null;
}
type EntryGroupGroup = KeyValuePair<string, Array<EntryGroup>>;
type EntryGroup = KeyValuePair<string, Array<LoggedActionEntryViewModel>>;
</script>

<style scoped lang="scss">
.progress
{
    max-width: 1280px;
    margin: auto;
    margin-top: 40px;
    margin-bottom: 40px;
    cursor: pointer;
}

.endpoint-group
{
    border-left: 5px solid gray;
    padding: 10px;
    margin-bottom: 40px;
    
    .endpoint-group-header
    {
        font-size: 22px;
    }

    .endpoint-subgroup
    {
        border-left: 5px solid lightblue;
        padding: 10px;
        margin-bottom: 30px;
    
        .endpoint-subgroup-header
        {
            font-size: 18px;
        }
    }
}
.endpoint
{
    border-left: 5px solid lightgreen;
    padding: 10px;
    margin-bottom: 20px;
}
</style>

<style>
</style>