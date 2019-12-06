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

            <div v-if="entries.length > 0" class="filter">
                <progress-bar-component class="progress elevation-4" 
                    :max="progressBarMax" 
                    :success="progressBarSuccess" 
                    :error="progressBarError"
                    v-on:clickedSuccess="showOnlyState(STATE_SUCCESS)"
                    v-on:clickedError="showOnlyState(STATE_ERROR)"
                    v-on:clickedRemaining="showOnlyState(STATE_UNDETERMINED)" />
      
                <v-layout row wrap>
                    <v-flex xs12>
                        <v-checkbox v-model="visibleStates" label="Successes" :value="STATE_SUCCESS" style="display:inline-block" class="mr-2"></v-checkbox>
                        <v-checkbox v-model="visibleStates" label="Errors" :value="STATE_ERROR" style="display:inline-block" class="mr-2"></v-checkbox>
                        <v-checkbox v-model="visibleStates" label="Undetermined" :value="STATE_UNDETERMINED" style="display:inline-block" class="mr-4"></v-checkbox>
                        
                        <v-checkbox
                            v-for="(verb, index) in verbs"
                            :key="`verb-${index}`"
                            v-model="visibleVerbs" :label="verb" :value="verb"
                            style="display:inline-block" class="mr-2"></v-checkbox>
                    </v-flex>
                </v-layout>

                <div>
                    Order by:
                    <v-btn x-small @click="setSortOrder(sortOption)"
                        v-for="(sortOption, index) in sortOptions"
                        :key="`sortOption-${index}`"
                        :disabled="currentlySortedBy == sortOption">
                        {{ sortOption.name }}
                    </v-btn>
                </div>
                <br />

                <v-checkbox v-model="groupEntries" label="Enable grouping" style="display:inline-block" class="mr-4"></v-checkbox>
                <a @click="clearFilteredIpAddress()" v-if="filteredIPAddress != null" class="filtere-address-filter mr-2">
                    Filtered to source IP: {{ filteredIPAddress }}
                    <v-icon size="20px">delete</v-icon>
                </a>
                <v-btn small @click="resetFilters" class="reset-filters-button">Reset filters</v-btn>
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
                                :isGrouped="true"
                                v-on:IPClicked="onIPClicked"
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
                        :isGrouped="false"
                        v-on:IPClicked="onIPClicked"
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
import LoggedEndpointDefinitionViewModel from '../../models/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from '../../models/RequestLog/LoggedEndpointRequestViewModel';
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

    entries: Array<LoggedEndpointDefinitionViewModel> = [];
    verbs: Array<string> = [];
    versions: Array<string> = [];
    sortOptions: Array<OrderByOption> = [
        {
            id: "latest-first",
            name: "Latest calls",
            sortedBy: (x: LoggedEndpointDefinitionViewModel) => Math.max(
                ...x.Calls.map(c => c.Timestamp.getTime()).concat(x.Errors.map(c => c.Timestamp.getTime()))
            ),
            thenBy: null,
            state: null
        },
        {
            id: "successes-first",
            name: "Successes",
            sortedBy: (x: LoggedEndpointDefinitionViewModel) =>
                x.Errors.length > 0 ? -99999 : x.Calls.length,
            thenBy: null,
            state: EntryState.Success
        },
        {
            id: "errors-first",
            name: "Errors",
            sortedBy: (x: LoggedEndpointDefinitionViewModel) => x.Errors.length,
            thenBy: (x: LoggedEndpointDefinitionViewModel) => x.Calls.length,
            state: EntryState.Error
        },
        {
            id: "untested-first",
            name: "Untested",
            sortedBy: (x: LoggedEndpointDefinitionViewModel) => -(x.Calls.length + x.Errors.length),
            thenBy: (x: LoggedEndpointDefinitionViewModel) => x.Errors.length,
            state: EntryState.Undetermined
        }
    ];
    visibleStates: Array<EntryState> = [ EntryState.Success, EntryState.Error, EntryState.Undetermined ];
    visibleVerbs: Array<string> = [];
    filteredIPAddress: string | null = null;
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

    get filteredEntries(): Array<LoggedEndpointDefinitionViewModel> {
        return this.entries
            .filter(x => this.visibleStates.indexOf(x.State) !== -1)
            .filter(x => this.visibleVerbs.indexOf(x.HttpVerb) !== -1)
            .filter(x => this.filteredIPAddress == null || x.Calls.some(c => c.SourceIP === this.filteredIPAddress))
            .sort((a, b) =>
                LinqUtils.SortByThenBy(a, b, this.currentlySortedBy.sortedBy, this.currentlySortedBy.thenBy));
    }

    get groupedFilteredEntries(): Array<EntryGroupGroup> {
        let groups: Array<EntryGroup> = LinqUtils.GroupByIntoKVP(
            this.filteredEntries,
            x => x.Group || "Other"
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
    includeEntryInProgress(entry: LoggedEndpointDefinitionViewModel): boolean {
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
        // .then(response => new Promise<Array<LoggedEndpointDefinitionViewModel>>(resolve => setTimeout(() => resolve(response), 3000)))
        .then((events: Array<LoggedEndpointDefinitionViewModel>) => this.onEventDataRetrieved(events))
        .catch((e) => {
            this.requestLogDataLoadInProgress = false;
            this.requestLogDataLoadFailed = true;
            this.requestLogDataFailedErrorMessage = `Failed to load data with the following error. ${e}.`;
            console.error(e);
        });
    }
    
    onEventDataRetrieved(events: Array<LoggedEndpointDefinitionViewModel>): void {
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
        this.verbs = this.verbs.map(x => x.length == 0 ? 'Unknown' : x);
        this.verbs.forEach(verb => this.visibleVerbs.push(verb));

        this.setFromUrl(originalUrlHashParts);
        this.requestLogDataLoadInProgress = false;
    }
    
    showOnlyState(state: EntryState): void {
        this.visibleStates = [ state ];
        this.currentlySortedBy = this.sortOptions.find(x => x.state == state) || this.currentlySortedBy;
        this.updateUrl();
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
        
        const sortById = parts.filter(x => x.startsWith('sort-')).map(x => x.substring(5))[0];
        const sortBy = this.sortOptions.find(x => x.id === sortById);
        if (sortBy !== undefined) {
            this.setSortOrder(sortBy);
        }

        const visible = parts.filter(x => x.startsWith('states-')).map(x => x.substring(7))[0];
        if (visible !== undefined && visible !== '.') {
            this.visibleStates = visible.split('.').filter(x => x.length > 0).map(x => parseInt(x));
        }

        this.groupEntries = !parts.some(x => x === 'no-group');

        const verbs = parts.filter(x => x.startsWith('verbs-')).map(x => x.substring(6))[0];
        if (verbs !== undefined && verbs !== '.') {
            this.visibleVerbs = verbs.split('.').map(x => x.toUpperCase());
        }

        const ip = parts.filter(x => x.startsWith('ip-')).map(x => x.substring(3))[0];
        if (ip !== undefined && ip.length > 0) {
            this.filteredIPAddress = ip;
        }
    }

    updateUrl(parts?: Array<string> | null): void {
        if (parts == null)
        {
            parts = ['requestlog'];

            if (this.currentlySortedBy.id != this.sortOptions[0].id)
            {
                parts.push(`sort-${this.currentlySortedBy.id}`);
            }
            if (this.visibleStates.length !== 3 && this.visibleStates.length > 0)
            {
                parts.push(`states-${this.visibleStates.join(".")}`);
            }
            if (!this.groupEntries)
            {
                parts.push(`no-group`);
            }
            if (this.visibleVerbs.length !== this.verbs.length && this.visibleVerbs.length > 0)
            {
                parts.push(`verbs-${this.visibleVerbs.map(x => x.toLowerCase()).join('.')}`);
            }
            if (this.filteredIPAddress != null && this.filteredIPAddress.length > 0)
            {
                parts.push(`ip-${this.filteredIPAddress}`);
            }
        }

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

    clearFilteredIpAddress(): void {
        this.filteredIPAddress = null;
        this.updateUrl();
    }

    resetFilters(): void {
        this.visibleStates = [ EntryState.Success, EntryState.Error, EntryState.Undetermined ];
        this.groupEntries = true;
        this.visibleVerbs = this.verbs;
        this.currentlySortedBy = this.sortOptions[0];
        this.filteredIPAddress = null;
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

    onIPClicked(ip: string): void {
        this.filteredIPAddress = ip;
        this.updateUrl();
    }
}
type NumberGetter<T> = (a: T) => number;
interface OrderByOption {
    id: string;
    name: string;
    sortedBy: NumberGetter<LoggedEndpointDefinitionViewModel>;
    thenBy: NumberGetter<LoggedEndpointDefinitionViewModel> | null;
    state: EntryState | null;
}
type EntryGroupGroup = KeyValuePair<string, Array<EntryGroup>>;
type EntryGroup = KeyValuePair<string, Array<LoggedEndpointDefinitionViewModel>>;
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
    border-left: 5px solid var(--v-secondary-lighten5);
    padding: 10px;
    margin-bottom: 40px;
    
    .endpoint-group-header
    {
        font-size: 22px;
    }

    .endpoint-subgroup
    {
        border-left: 5px solid var(--v-accent-lighten4);
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
    border-left: 5px solid var(--v-success-lighten2);
    padding: 10px;
    margin-bottom: 20px;
}
.filtere-address-filter {
    vertical-align: text-bottom;
}
.reset-filters-button {
    vertical-align: super;
}
</style>

<style>
</style>