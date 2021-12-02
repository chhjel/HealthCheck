<!-- src/components/modules/DataRepeater/DataRepeaterPageComponent.vue -->
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
                    :hrefKey="`Href`"
                    :filterKeys="[ 'Name', 'Description' ]"
                    :loading="metadataLoadStatus.inProgress"
                    :disabled="isLoading"
                    ref="filterableList"
                    v-on:itemClicked="onMenuItemClicked"
                    @itemMiddleClicked="onMenuItemMiddleClicked"
                    />
            </v-navigation-drawer>
            
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        <v-container>
                            <div v-if="selectedStream && selectedItemId == null">
                                <h2 v-if="selectedStream.StreamItemsName">{{ selectedStream.StreamItemsName }}</h2>
                                <p v-if="selectedStream.Description" v-html="selectedStream.Description"></p>

                                <div class="data-repeater-filters">
                                    <v-text-field
                                        v-model="filterItemId"
                                        @blur="onFilterChanged"
                                        @keyup.enter="onFilterChanged"
                                        label="Filter"
                                        clearable
                                        class="filter-input"
                                        :readonly="isLoading"
                                    ></v-text-field>
                                </div>
                                <div class="data-repeater-filters">
                                    <v-checkbox
                                        :value="filterRetryAllowedBinding"
                                        :indeterminate="filterRetryAllowed == null" 
                                        :label="filterRetryAllowedLabel"
                                        :disabled="isLoading"
                                        @click="setNextFilterRetryAllowedState"
                                        color="secondary"
                                    ></v-checkbox>
                                    <v-combobox
                                        v-model="filterTags"
                                        @blur="onFilterChanged"
                                        @keyup.enter="onFilterChanged"
                                        :items="tagPresets"
                                        label="Included tags"
                                        multiple
                                        chips
                                        clearable
                                        class="filter-input"
                                        :readonly="isLoading"
                                        ></v-combobox>
                                </div>

                                <v-btn @click="loadCurrentStreamItems" :disabled="isLoading" class="right">
                                    <v-icon size="20px" class="mr-2">refresh</v-icon>Refresh
                                </v-btn>

                                <paging-component
                                    :count="totalResultCount"
                                    :pageSize="pageSize"
                                    v-model="pageIndex"
                                    :asIndex="true"
                                    class="mb-2 mt-2"
                                    />
                            </div>

                            <!-- LOAD PROGRESS -->
                            <v-progress-linear 
                                v-if="isLoading"
                                indeterminate color="green"></v-progress-linear>

                            <!-- DATA LOAD ERROR -->
                            <v-alert :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
                            {{ dataLoadStatus.errorMessage }}
                            </v-alert>

                            <div v-if="selectedStream && selectedItemId == null">
                                <p>{{ totalResultCount}} matches</p>
                                <div style="clear: both"></div>
                                <div>
                                    <div v-for="(item, iIndex) in items"
                                        :key="`item-${iIndex}-${item.Id}`"
                                        @click="setActiveItemId(item.Id)"
                                        @click.middle.stop.prevent="onItemClickedMiddle(item)"
                                        @mousedown.middle.stop.prevent
                                        @keyup.enter="setActiveItemId(item.Id)"
                                        class="data-repeater-list-item"
                                        :class="itemRowClasses(item)"
                                        tabindex="0">
                                        <span class="data-repeater-list-item--title">{{ item.ItemId }}</span>
                                        <span v-if="item.Summary" class="data-repeater-list-item--summary">{{ item.Summary }}</span>
                                        <div class="data-repeater-list-item--spacer"></div>
                                        <span class="data-repeater-list-item--timestamp">{{ formatDate(item.InsertedAt) }}</span>
                                        <div class="data-repeater-list-item--break"></div>
                                        <v-tooltip v-if="item.AllowRetry" bottom>
                                            <template v-slot:activator="{ on }">
                                                <span class="data-repeater-list-item--icon" style="cursor: help;" v-on="on"><v-icon>replay</v-icon></span>
                                            </template>
                                            <span>Can be attempted retried</span>
                                        </v-tooltip>
                                        <v-tooltip v-if="item.ExpiresAt && expiresSoon(item.ExpiresAt)" bottom>
                                            <template v-slot:activator="{ on }">
                                                <span class="data-repeater-list-item--icon" style="cursor: help;" v-on="on"><v-icon>timer</v-icon></span>
                                            </template>
                                            <span>Expires soon</span>
                                        </v-tooltip>

                                        <div class="data-repeater-list-item--tags">
                                            <div class="data-repeater-list-item--tag"
                                                v-for="(tag, tIndex) in item.Tags"
                                                :key="`item-${iIndex}-${item.Id}-tag-${tIndex}`">{{ tag }}</div>
                                        </div>
                                    </div>
                                </div>
                                
                                <paging-component
                                    :count="totalResultCount"
                                    :pageSize="pageSize"
                                    v-model="pageIndex"
                                    :asIndex="true"
                                    class="mb-2 mt-2"
                                    />
                            </div>

                            <!-- ITEM -->
                            <div v-if="selectedItemId">
                                <data-repeater-item-component
                                    :itemId="selectedItemId"
                                    :stream="selectedStream"
                                    :config="config"
                                    :options="options"
                                    @change="onItemUpdated"
                                    @close="setActiveItemId(null)" />
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
import FrontEndOptionsViewModel from  '../../../models/Common/FrontEndOptionsViewModel';
import FilterableListComponent, { FilterableListItem } from  '../../Common/FilterableListComponent.vue';
import { FetchStatus } from  '../../../services/abstractions/HCServiceBase';
import ModuleConfig from  '../../../models/Common/ModuleConfig';
import ModuleOptions from  '../../../models/Common/ModuleOptions';
import DataRepeaterService from  '../../../services/DataRepeaterService';
import { HCGetDataRepeaterStreamDefinitionsViewModel } from "generated/Models/Core/HCGetDataRepeaterStreamDefinitionsViewModel";
import { HCDataRepeaterStreamViewModel } from "generated/Models/Core/HCDataRepeaterStreamViewModel";
import BackendInputComponent from "components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { HCDataRepeaterStreamItemViewModel } from "generated/Models/Core/HCDataRepeaterStreamItemViewModel";
import DataRepeaterItemComponent from "./DataRepeaterItemComponent.vue";
import PagingComponent from "../../Common/Basic/PagingComponent.vue";
import HashUtils from "../../../util/HashUtils";
import { HCDataRepeaterStreamItemsPagedViewModel } from "generated/Models/Core/HCDataRepeaterStreamItemsPagedViewModel";
import { Route } from "vue-router";
import DateUtils from "util/DateUtils";
import UrlUtils from "util/UrlUtils";
import { HCDataRepeaterStreamItemStatus } from "generated/Enums/Core/HCDataRepeaterStreamItemStatus";

@Component({
    components: {
        FilterableListComponent,
        BackendInputComponent,
        DataRepeaterItemComponent,
        PagingComponent
    }
})
export default class DataRepeaterPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    // Service
    service: DataRepeaterService = new DataRepeaterService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();
    metadataLoadStatus: FetchStatus = new FetchStatus();

    streamDefinitions: HCGetDataRepeaterStreamDefinitionsViewModel | null = null;
    selectedStream: HCDataRepeaterStreamViewModel | null = null;
    selectedItemId: string | null = null;
    actionParameters: any = {};
    items: Array<HCDataRepeaterStreamItemViewModel> = [];
    tagPresets: Array<string> = [];

    // Filter/pagination
    pageIndex: number = 0;
    pageSize: number = 50;
    filterItemId: string = '';
    filterRetryAllowed: boolean | null = null;
    filterRetryAllowedBinding: boolean | null = null;
    filterTags: Array<string> = [];
    totalResultCount: number = 0;
    filterCache: any = {};

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.$store.commit('showMenuButton', true);

        this.resetFilter();
        this.loadStreamDefinitions();

        setTimeout(() => {
            this.routeListener = this.$router.afterEach((t, f) => this.onRouteChanged(t, f));
        }, 100);
    }

    routeListener: Function | null = null;
    beforeDestroy(): void {
        if (this.routeListener)
        {
            this.routeListener();
        }
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }
    
    get isLoading(): boolean {
        return this.metadataLoadStatus.inProgress || this.dataLoadStatus.inProgress;
    }
    
    get menuItems(): Array<FilterableListItem>
    {
        if (!this.streamDefinitions) return [];
        return this.streamDefinitions.Streams.map(x => {
            let d = {
                title: x.Name,
                subtitle: '',
                data: x
            };
            (<any>d)['Href'] = "/woot";
            return d;
        });
    }
    
    get filterRetryAllowedLabel(): string {
        if (this.filterRetryAllowed == null) return 'Retryable & non-retryable';
        else if (this.filterRetryAllowed == true) return 'Retryable only';
        else return 'Non-retryable only';
    }

    ////////////////////
    //  Parent Menu  //
    //////////////////
    drawerState: boolean = this.storeMenuState;
    get storeMenuState(): boolean {
        return this.$store.state.ui.menuExpanded;
    }
    @Watch("storeMenuState")
    onStoreMenuStateChanged(): void {
        this.drawerState = this.storeMenuState;
    }

    ////////////////
    //  METHODS  //
    //////////////
    resetFilter(): void {
        this.pageIndex = 0;
        this.pageSize = 50;
        this.filterItemId = '';
        this.filterRetryAllowed = null;
        this.filterRetryAllowedBinding = false;
        this.tagPresets = this.selectedStream?.FilterableTags || [];
        this.filterTags = this.selectedStream?.InitiallySelectedTags || [];
    }

    loadStreamDefinitions(): void {
        this.service.GetStreamDefinitions(this.metadataLoadStatus, {
            onSuccess: (data) => this.onStreamDefinitionsRetrieved(data)
        });
    }

    onStreamDefinitionsRetrieved(data: HCGetDataRepeaterStreamDefinitionsViewModel | null): void {
        this.streamDefinitions = data;
        this.streamDefinitions?.Streams.forEach(s => {
            s.Actions.forEach(a => {
                this.actionParameters[a.Id] = [];
            });
        });

        const idFromHash = this.$route.params.streamId;
        if (this.streamDefinitions)
        {
            const matchingStream = this.streamDefinitions.Streams.filter(x => this.hash(x.Id) == idFromHash)[0];
            if (matchingStream) {
                this.setActiveStream(matchingStream, false);
            } else if (this.streamDefinitions.Streams.length > 0) {
                this.setActiveStream(this.streamDefinitions.Streams[0]);   
            }
        }
    }

    setActiveStream(stream: HCDataRepeaterStreamViewModel | null, updateUrl: boolean = true): void {
        if (this.isLoading) {
            return;
        }

        const prevStreamid = this.selectedStream?.Id;

        this.selectedStream = stream;
        this.selectedItemId = null;
        (this.$refs.filterableList as FilterableListComponent).setSelectedItem(stream);
        if (stream == null)
        {
            return;
        }

        if (prevStreamid != null)
        {
            this.cacheFilter(prevStreamid);
        }

        this.resetFilter();
        if (this.applyFilterFromCache(stream.Id))
        {
            this.$nextTick(() => this.updateUrlFromFilter());
        } else {
            this.applyFilterFromUrl();
        }
        this.loadCurrentStreamItems();

        if (updateUrl && this.$route.params.streamId != this.hash(stream.Id))
        {
            this.$router.push(`/dataRepeater/${this.hash(stream.Id)}`);
        }
    }

    hash(input: string) { return HashUtils.md5(input); }

    loadCurrentStreamItems(): void {
        if (!this.selectedStream) return;

        this.service.GetStreamItemsPaged({
            StreamId: this.selectedStream.Id,
            Filter: this.filterItemId,
            PageIndex: this.pageIndex,
            PageSize: this.pageSize,
            Tags: this.filterTags,
            RetryAllowed: this.filterRetryAllowed == null ? undefined : this.filterRetryAllowed
        }, this.dataLoadStatus, {
            onSuccess: (data) => {
                if (data != null)
                {
                    this.onStreamItemsLoaded(data);
                }
            }
        })
    }

    onStreamItemsLoaded(data: HCDataRepeaterStreamItemsPagedViewModel): void {
        this.totalResultCount = data.TotalCount;
        this.items = data.Items;
        
        const idFromHash = this.$route.params.itemId;
        this.setActiveItemId(idFromHash, false);
    }

    onFilterChanged(): void {
        this.updateUrlFromFilter();
        this.loadCurrentStreamItems();
    }

    updateUrlFromFilter(): void {
        let query: any = {};

        if (this.filterItemId != '')
        {
            query.q = this.filterItemId;
        }
        if (this.filterRetryAllowed != null)
        {
            query.r = this.filterRetryAllowed;
        }

        const defaultTags = this.selectedStream?.InitiallySelectedTags || [];
        if (this.filterTags.length != defaultTags.length || !this.filterTags.every(x => defaultTags.includes(x)))
        {
            query.t = (this.filterTags.length == 0) ? '_' : this.filterTags;
        }

        if (JSON.stringify(query) != JSON.stringify(this.$route.query))
        {
            this.$router.replace({ query: query });
        }
    }

    private hasAppliedFromUrl: boolean = false;
    applyFilterFromUrl(): void {
        if (this.hasAppliedFromUrl) {
            return;
        }
        this.hasAppliedFromUrl = true;

        if (this.$route.query.q)
        {
            this.filterItemId = this.$route.query.q as string || '';
        }

        const retryAllowed = this.$route.query.r as string | boolean;
        if (retryAllowed != null)
        {
            this.$nextTick(() => {
                this.filterRetryAllowed = retryAllowed == 'true' || retryAllowed == true;
                this.filterRetryAllowedBinding = this.filterRetryAllowed;
            });
        }

        const tags = this.$route.query.t;
        if (tags != null)
        {
            if (typeof tags == 'string')
            {
                this.filterTags = tags == '_' ? [] : [tags];
            }
            else if (Array.isArray(tags))
            {
                this.filterTags = (<string[]>tags).filter(x => x != '_');
            }
        }
    }

    cacheFilter(streamId: string): void {
        this.filterCache[streamId] = {
            q: this.filterItemId,
            r: this.filterRetryAllowed,
            t: this.filterTags
        };
    }

    applyFilterFromCache(streamId: string): boolean {
        const cache = this.filterCache[streamId];
        if (cache == null) return false;
        
        this.filterItemId = this.filterCache[streamId].q || '';

        if (this.filterCache[streamId].r != null)
        {
            this.filterRetryAllowed = this.filterCache[streamId].r == 'true' || this.filterCache[streamId].r == true;
            this.filterRetryAllowedBinding = this.filterRetryAllowed;
        }
        this.filterTags = this.filterCache[streamId].t || [];

        return true;
    }

    setActiveItemId(itemId: string | null, updateUrl: boolean = true): void {
        if (this.selectedItemId == itemId) {
            return;
        }

        this.selectedItemId = itemId;

        if (updateUrl)
        {
            const streamId = this.selectedStream?.Id || '';
            const itemIdParam = itemId == null ? '' : `/${itemId}`;
            this.$router.push(`/dataRepeater/${this.hash(streamId)}${itemIdParam}`);
        }
    }

    onItemClickedMiddle(item: HCDataRepeaterStreamItemViewModel): void {
        const route = `#/dataRepeater/${this.hash(this.selectedStream?.Id||'')}/${item.Id}`;
        UrlUtils.openRouteInNewTab(route);
    }

    setNextFilterRetryAllowedState(): void {
        if (this.isLoading)
        {
            return;
        }
        
        this.$nextTick(() => {
            if (this.filterRetryAllowed == null) {
                this.filterRetryAllowed = true;
                this.filterRetryAllowedBinding = true;
            } else if (this.filterRetryAllowed == true) {
                this.filterRetryAllowed = false;
                this.filterRetryAllowedBinding = false;
            } else {
                this.filterRetryAllowed = null
                this.filterRetryAllowedBinding = false;
            }
            this.onFilterChanged();
        });
    }

    formatDate(date: Date): string {
        return DateUtils.FormatDate(date, "dd/MM/yy HH:mm:ss");
    }

    itemRowClasses(item: HCDataRepeaterStreamItemViewModel): any {
        let success = item.LastRetryWasSuccessful;
        let failed = !success && (item.LastRetryWasSuccessful == false || item.Error || item.FirstError);
        if (item.ForcedStatus)
        {
            success = item.ForcedStatus == HCDataRepeaterStreamItemStatus.Success;
            failed = item.ForcedStatus == HCDataRepeaterStreamItemStatus.Error;
        }
        return {
            'retry-success': success,
            'retry-failed': failed
        };
    }

    expiresSoon(dateStr: string): boolean {
        const d = new Date(dateStr);
        const diff = d.getTime() - new Date().getTime();
        const threshold = 60 * 60 * 1000; // 60 minutes
        if (diff < threshold)
        {
            return true;
        }
        return false;
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMenuItemClicked(item: FilterableListItem): void {
        if (this.selectedItemId != null) {
            this.setActiveItemId(null);
        }
        this.setActiveStream(item.data);
    }

    onMenuItemMiddleClicked(item: FilterableListItem): void {
        if (item && item.data && item.data.Id)
        {
            const idHash = this.hash(item.data.Id);
            const route = `#/dataRepeater/${idHash}`;
            UrlUtils.openRouteInNewTab(route);
        }
    }

    @Watch("pageIndex")
    onPageIndexChanged(): void {
        this.loadCurrentStreamItems();
    }

    onRouteChanged(to: Route, from: Route): void {
        if (!this.streamDefinitions) return;

        const oldStreamIdFromHash = from.params.streamId || null;
        const newStreamIdFromHash = to.params.streamId || null;
        const streamChanged = oldStreamIdFromHash != newStreamIdFromHash;

        const oldItemIdFromHash = from.params.itemId || null;
        const newItemIdFromHash = to.params.itemId || null;
        const itemChanged = oldItemIdFromHash != newItemIdFromHash;

        if (streamChanged)
        {
            const matchingStream = this.streamDefinitions.Streams.filter(x => this.hash(x.Id) == newStreamIdFromHash)[0] || null;
            this.setActiveStream(matchingStream, false);
            this.setActiveItemId(null, false);
        }
        else if (itemChanged)
        {
            this.setActiveItemId(newItemIdFromHash, false);
        }
    }

    onItemUpdated(item: HCDataRepeaterStreamItemViewModel): void {
        Vue.set(this.items, this.items.findIndex(x => x.Id == item.Id), item)
    }
}
</script>

<style scoped lang="scss">
.menu {
    box-shadow: 0 2px 2px -1px rgba(0, 0, 0, 0.02), 0 3px 2px 0 rgba(0, 0, 0, 0.02), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
}
@media (max-width: 960px) {
    .menu-items { 
        margin-top: 67px;
    }
}
.data-repeater-filters {
    display: flex;
    flex-direction: row;
    flex-wrap: wrap;

    .v-text-field {
        margin-right: 10px;
    }
    .v-input--checkbox {
        width: 280px;
        flex-grow: inherit;
        flex-shrink: inherit;
    }
}
.data-repeater-list-item {
    cursor: pointer;
    display: flex;
    flex-direction: row;
    flex-wrap: wrap;
    padding: 5px;
    align-items: baseline;
    border-left: 4px solid #d5d5d5;

    &.retry-success {
        border-left: 4px solid #97e197;
    }
    &.retry-failed {
        border-left: 4px solid #e19797;
    }

    &:not(:first-child)
    {
        border-top: 1px solid gray;
    }
    &:nth-child(even)
    {
        background-color: #eee;
    }
    &:hover {
        background-color: #ddd;
        
        .data-repeater-list-item--tag {
            background-color: #c3c3c3;
        }
    }
    &:focus, :active {
        background-color: #d5d5d5;
    }
    &--icon {
        margin-right: 5px;
        i {
            height: 22px;
            padding-top: 1px;
            color: rgba(0,0,0,0.20);
        }
    }
    &--timestamp {
        margin-right: 10px;
        font-size: 12px;
    }
    &--title {
        font-weight: 600;
        margin-right: 10px;
    }
    &--summary {
        text-overflow: ellipsis;
        overflow: hidden;
        white-space: nowrap;
        margin-right: 10px;
        font-size: 12px;
    }
    &--tags {
        display: flex;
        flex-wrap: wrap;
    }
    &--tag {
        padding: 3px 6px;
        background-color: #dcdcdc;
        border-radius: 3px;
        margin-right: 5px;
        margin-top: 5px;
        font-size: 12px;
    }
    &--break {
        flex-basis: 100%;
        height: 0;
    }
    &--spacer {
        flex: 1;
    }
}
</style>