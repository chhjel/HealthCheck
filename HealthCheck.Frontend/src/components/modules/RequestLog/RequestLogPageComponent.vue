<!-- src/components/modules/RequestLog/RequestLogPageComponent.vue -->
<template>
    <div>
        <div> <!-- PAGE-->
        <div fluid fill-height class="content-root">
        <div>
        <div class="pl-4 pr-4 pb-4">
          <!-- CONTENT BEGIN -->
            
        <div grid-list-md>
            <div align-content-center wrap v-if="loadStatus.inProgress || loadStatus.failed">
                <!-- LOAD ERROR -->
                <alert-component
                    :value="loadStatus.failed"
                    type="error">
                {{ loadStatus.errorMessage }}
                </alert-component>

                <!-- PROGRESS BAR -->
                <progress-linear-component
                    v-if="loadStatus.inProgress"
                    indeterminate color="green"></progress-linear-component>
            </div>

            <div align-content-center wrap v-if="entries.length == 0 && !loadStatus.inProgress && !loadStatus.failed">
                <alert-component type="info" :value="true">
                    No requests has been logged yet.
                </alert-component>
            </div>

            <div v-if="entries.length > 0" class="filter">
                <progress-bar-component class="progress elevation-4" 
                    :max="progressBarMax" 
                    :success="progressBarSuccess" 
                    :error="progressBarError"
                    v-on:clickedSuccess="showOnlyState(STATE_SUCCESS)"
                    v-on:clickedError="showOnlyState(STATE_ERROR)"
                    v-on:clickedRemaining="showOnlyState(STATE_UNDETERMINED)" />
      
                <div row wrap>
                    <div xs12>
                        <checkbox-component v-model:value="visibleStates" label="Successes" :value="STATE_SUCCESS" style="display:inline-block" class="mr-2"></checkbox-component>
                        <checkbox-component v-model:value="visibleStates" label="Errors" :value="STATE_ERROR" style="display:inline-block" class="mr-2"></checkbox-component>
                        <checkbox-component v-model:value="visibleStates" label="Not Called" :value="STATE_UNDETERMINED" style="display:inline-block" class="mr-4"></checkbox-component>
                        
                        <checkbox-component
                            v-for="(verb, index) in verbs"
                            :key="`verb-${index}`"
                            v-model:value="visibleVerbs" :label="verb" :value="verb"
                            style="display:inline-block" class="mr-2"></checkbox-component>
                    </div>
                </div>

                <div>
                    Order by:
                    <btn-component x-small @click="setSortOrder(sortOption)"
                        v-for="(sortOption, index) in sortOptions"
                        :key="`sortOption-${index}`"
                        :disabled="currentlySortedBy == sortOption">
                        {{ sortOption.name }}
                    </btn-component>
                </div>
                <br />

                <checkbox-component v-model:value="groupEntries" label="Enable grouping" style="display:inline-block" class="mr-4"></checkbox-component>
                <a @click="clearFilteredIpAddress()" v-if="filteredIPAddress != null" class="filtere-address-filter mr-2">
                    Filtered to source IP: {{ filteredIPAddress }}
                    <icon-component size="20px">delete</icon-component>
                </a>
                <btn-component small @click="resetFilters" class="reset-filters-button">Reset filters</btn-component>
                <br />

                <!-- Versions:
                <div v-for="(version, index) in versions"
                     :key="`version-${index}`">
                    <checkbox-component v-model:value="visibleVersions" :label="version" :value="version"></checkbox-component>
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
                                :entry="entry"
                                :isGrouped="true"
                                v-on:IPClicked="onIPClicked"
                                v-on:IdClicked="onIdClicked"
                                />
                        </div>
                    </div>
                </div>

                <div v-if="!groupEntries">
                    <request-endpoint-component
                        v-for="(entry, index) in filteredEntries"
                        :key="`entry-${index}`"
                        class="endpoint"
                        :entry="entry"
                        :isGrouped="false"
                        v-on:IPClicked="onIPClicked"
                        v-on:IdClicked="onIdClicked"
                        />
                </div>

                <div row wrap v-if="hasAccessToClearRequestLog">
                    <div xs12 sm6 md4>
                        <btn-component
                            :loading="clearStatus.inProgress"
                            :disabled="clearStatus.inProgress"
                            color="error"
                            @click="clearRequestLog(true)"
                            >
                            <icon-component size="20px" class="mr-2">delete_forever</icon-component>
                            Clear requests + definitions
                        </btn-component>
                    </div>

                    <div xs12 sm6 md4>
                        <btn-component
                            :loading="clearStatus.inProgress"
                            :disabled="clearStatus.inProgress"
                            color="error"
                            @click="clearRequestLog(false)"
                            >
                            <icon-component size="20px" class="mr-2">delete</icon-component>
                            Clear requests
                        </btn-component>
                    </div>

                    <div xs12 v-if="clearStatus.failed">
                        <alert-component
                            :value="clearStatus.failed"
                            type="error">
                        {{ clearStatus.errorMessage }}
                        </alert-component>
                    </div>
                </div>
            </div>
        </div>


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
import LoggedEndpointDefinitionViewModel from '@models/modules/RequestLog/LoggedEndpointDefinitionViewModel';
import LoggedEndpointRequestViewModel from '@models/modules/RequestLog/LoggedEndpointRequestViewModel';
import RequestEndpointComponent from '@components/modules/RequestLog/RequestEndpointComponent.vue';
import ProgressBarComponent from '@components/Common/ProgressBarComponent.vue';
import { EntryState } from '@models/modules/RequestLog/EntryState';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';
import KeyArray from '@util/models/KeyArray';
import KeyValuePair from '@models/Common/KeyValuePair';
import RequestLogService from '@services/RequestLogService';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import ModuleOptions from '@models/Common/ModuleOptions';
import ModuleConfig from '@models/Common/ModuleConfig';
import { StoreUtil } from "@util/StoreUtil";

@Options({
    components: {
        ProgressBarComponent,
        RequestEndpointComponent
    }
})
export default class RequestLogPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;

    service: RequestLogService = new RequestLogService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    loadStatus: FetchStatus = new FetchStatus();
    clearStatus: FetchStatus = new FetchStatus();

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
            name: "Not called",
            sortedBy: (x: LoggedEndpointDefinitionViewModel) => -(x.Calls.length + x.Errors.length),
            thenBy: (x: LoggedEndpointDefinitionViewModel) => x.Errors.length,
            state: EntryState.Undetermined
        }
    ];
    visibleStates: Array<EntryState> = [ EntryState.Success, EntryState.Error, EntryState.Undetermined ];
    visibleVerbs: Array<string> = [];
    filteredIPAddress: string | null = null;
    currentlySortedBy: OrderByOption = this.sortOptions[0];
    currentScrollTarget: string | null = null;
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
    get hasAccessToClearRequestLog(): boolean {
        return this.options.AccessOptions.indexOf('ClearLog') != -1;
    }
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }
    
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
        this.service.GetRequestLog(this.loadStatus, { onSuccess: (data) => this.onEventDataRetrieved(data)});
    }
    
    onEventDataRetrieved(events: Array<LoggedEndpointDefinitionViewModel>): void {
        // const originalUrlHashParts = UrlUtils.GetHashParts();

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
                }).sort((a, b) => LinqUtils.SortBy(a, b, (item => item.Timestamp.getTime()))),
                Errors: x.Errors.map(c => {
                    return {
                        ...c,
                        Timestamp: new Date(c.Timestamp)
                    }
                }).sort((a, b) => LinqUtils.SortBy(a, b, (item => item.Timestamp.getTime())))
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

        // this.setFromUrl(originalUrlHashParts);
        this.loadStatus.inProgress = false;
    }

    clearRequestLog(includeDefinitions: boolean): void {
        this.service.ClearRequestLog(includeDefinitions, this.clearStatus, {
            onSuccess: (data) => {
                if (includeDefinitions)
                {
                    this.entries = [];
                }
                else
                {
                    this.entries = this.entries.map(x => {
                        x.Calls = [];
                        x.Errors = [];
                        return x;
                    });
                }
            }
        });
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
        // const parts = forcedParts || UrlUtils.GetHashParts();
        
        // const sortById = parts.filter(x => x.startsWith('sort-')).map(x => x.substring(5))[0];
        // const sortBy = this.sortOptions.find(x => x.id === sortById);
        // if (sortBy !== undefined) {
        //     this.setSortOrder(sortBy);
        // }

        // const visible = parts.filter(x => x.startsWith('states-')).map(x => x.substring(7))[0];
        // if (visible !== undefined && visible !== '.') {
        //     this.visibleStates = visible.split('.').filter(x => x.length > 0).map(x => parseInt(x));
        // }

        // this.groupEntries = !parts.some(x => x === 'no-group');

        // const verbs = parts.filter(x => x.startsWith('verbs-')).map(x => x.substring(6))[0];
        // if (verbs !== undefined && verbs !== '.') {
        //     this.visibleVerbs = verbs.split('.').map(x => x.toUpperCase());
        // }

        // const ip = parts.filter(x => x.startsWith('ip-')).map(x => x.substring(3))[0];
        // if (ip !== undefined && ip.length > 0) {
        //     this.filteredIPAddress = ip;
        // }

        // const id = parts.filter(x => x.startsWith('id-')).map(x => x.substring(3))[0];
        // if (id !== undefined && id.length > 0) {
        //     this.currentScrollTarget = decodeURI(id);
        // }
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
            if (this.currentScrollTarget != null && this.currentScrollTarget.length > 0)
            {
                parts.push(`id-${encodeURI(this.currentScrollTarget)}`);
            }
        }

        // UrlUtils.SetHashParts(parts);
        
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
        this.currentScrollTarget = null;
    }

    scrollToEndpoint(endpointId: string | null): void {
        if (endpointId == null || endpointId.length == 0) {
            return;
        }

        setTimeout(() => {
            const testElement = document.querySelector(`[data-endpoint-id='${encodeURI(endpointId)}']`);
            if (testElement != null) {
                window.scrollTo({
                    top: (window.pageYOffset || document.documentElement.scrollTop) 
                        + testElement.getBoundingClientRect().top - 78,
                    behavior: 'smooth'
                });
            }
        }, 10);
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

    @Watch("currentScrollTarget")
    onCurrentScrollTargetChanged(): void {
        this.updateUrl();
        this.scrollToEndpoint(this.currentScrollTarget);
    }

    onIPClicked(ip: string): void {
        this.filteredIPAddress = ip;
        this.updateUrl();
    }

    onIdClicked(id: string): void {
        this.currentScrollTarget = id;
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

        &:last-child {
            margin-bottom: 20px;
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