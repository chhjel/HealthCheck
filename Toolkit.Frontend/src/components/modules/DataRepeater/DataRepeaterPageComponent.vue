<!-- src/components/modules/DataRepeater/DataRepeaterPageComponent.vue -->
<template>
    <div>
        <div>
            <!-- NAVIGATION DRAWER -->
            <Teleport to="#module-nav-menu">
                <filterable-list-component
                    :items="menuItems"
                    :groupByKey="`GroupName`"
                    :sortByKey="`GroupName`"
                    :hrefKey="`Href`"
                    :filterKeys="[ 'Name', 'Description' ]"
                    :loading="metadataLoadStatus.inProgress"
                    :disabled="isLoading"
                    :showFilter="false"
                    :groupIfSingleGroup="false"
                    ref="filterableList"
                    v-on:itemClicked="onMenuItemClicked"
                    @itemMiddleClicked="onMenuItemMiddleClicked"
                    />
            </Teleport>
            
            
            <div class="content-root">
                <div v-if="selectedStream && selectedItemId == null">
                    <h2 v-if="selectedStream.StreamItemsName" class="mb-3">{{ selectedStream.StreamItemsName }}</h2>
                    <p v-if="selectedStream.Description" class="mb-3" v-html="selectedStream.Description"></p>

                    <div class="data-repeater-filters mb-2">
                        <text-field-component
                            v-model:value="filterItemId"
                            @blur="onFilterChanged"
                            @keyup.enter="onFilterChanged"
                            label="Filter"
                            clearable
                            class="filter-input"
                            style="flex:1"
                            :readonly="isLoading"
                        ></text-field-component>
                    </div>
                    <div class="data-repeater-filters flex layout">
                        <select-component
                            v-model:value="filterRetryAllowedBinding"
                            @blur="onFilterChanged"
                            :items="retryableChoices"
                            label="Included"
                            class="filter-input xs12 md4 mr-2 mb-2"
                            :readonly="isLoading"
                            ></select-component>
                        <select-component
                            v-model:value="filterTags"
                            @blur="onFilterChanged"
                            @keyup.enter="onFilterChanged"
                            :items="tagPresets"
                            label="Tags"
                            placeholder="All tags included"
                            multiple
                            clearable
                            allowInput allowCustom
                            class="filter-input xs12 md8 mb-2"
                            style="flex:1"
                            :readonly="isLoading"
                            ></select-component>
                    </div>

                    <div class="pagination-and-actions flex layout">
                        <paging-component
                            :count="totalResultCount"
                            :pageSize="pageSize"
                            v-model:value="pageIndex"
                            @change="onPageIndexChanged"
                            :asIndex="true"
                            class="mb-2 mt-2 xs12 md7"
                            style="padding-top: 6px"
                            />
                        <div class="spacer"></div>
                        <div class="flex flex-end flex-wrap">
                            <btn-component @click="loadCurrentStreamItems(true)" :disabled="isLoading">
                                <icon-component size="20px" class="mr-2">refresh</icon-component>Refresh
                            </btn-component>
                            <btn-component @click="batchActionsDialogVisible = true" :disabled="isLoading" v-if="hasBatchActions">
                                <icon-component size="20px" class="mr-2">checklist</icon-component>Batch actions
                            </btn-component>
                        </div>
                    </div>
                </div>

                <!-- LOAD PROGRESS -->
                <progress-linear-component 
                    v-if="isLoading"
                    indeterminate color="success"></progress-linear-component>

                <!-- DATA LOAD ERROR -->
                <alert-component :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
                {{ dataLoadStatus.errorMessage }}
                </alert-component>

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
                            <span class="data-repeater-list-item--icon"
                                title="Can be attempted retried"
                                style="cursor: help;" v-if="item.AllowRetry">
                                <icon-component size="22px" color="#00000033">replay</icon-component></span>
                            <span class="data-repeater-list-item--icon"
                                title="Expires soon"
                                style="cursor: help;" v-if="item.ExpiresAt && expiresSoon(item.ExpiresAt)">
                                <icon-component size="22px" color="#00000033">timer</icon-component></span>

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
                        v-model:value="pageIndex"
                        @change="onPageIndexChanged"
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
            </div>
          

          <!-- DIALOGS -->
            <dialog-component v-model:value="batchActionsDialogVisible"
                max-width="800"
                :persistent="dataLoadStatus.inProgress">
                <template #header>Batch actions</template>
                <template #footer>
                    <btn-component color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="batchActionsDialogVisible = false">Cancel</btn-component>
                </template>
                <div v-if="selectedStream && hasBatchActions">
                    <data-repeater-batch-action-component
                        v-for="(batchAction, baIndex) in selectedStream.BatchActions"
                        :key="`batch-action-${baIndex}-${batchAction.Id}`"
                        :config="config"
                        :stream="selectedStream"
                        :action="batchAction"
                        />
                </div>
            </dialog-component>
          <!-- DIALOGS END -->
        </div>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import FilterableListComponent from '@components/Common/FilterableListComponent.vue';
import { FilterableListItem } from '@components/Common/FilterableListComponent.vue.models';
import { FetchStatus } from '@services/abstractions/TKServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import DataRepeaterService from '@services/DataRepeaterService';
import { TKGetDataRepeaterStreamDefinitionsViewModel } from "@generated/Models/Core/TKGetDataRepeaterStreamDefinitionsViewModel";
import { TKDataRepeaterStreamViewModel } from "@generated/Models/Core/TKDataRepeaterStreamViewModel";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { TKDataRepeaterStreamItemViewModel } from "@generated/Models/Core/TKDataRepeaterStreamItemViewModel";
import DataRepeaterItemComponent from '@components/modules/DataRepeater/DataRepeaterItemComponent.vue';
import DataRepeaterBatchActionComponent from '@components/modules/DataRepeater/DataRepeaterBatchActionComponent.vue';
import PagingComponent from '@components/Common/Basic/PagingComponent.vue';
import HashUtils from '@util/HashUtils';
import { TKDataRepeaterStreamItemsPagedViewModel } from "@generated/Models/Core/TKDataRepeaterStreamItemsPagedViewModel";
import DateUtils from "@util/DateUtils";
import UrlUtils from "@util/UrlUtils";
import StringUtils from "@util/StringUtils";
import { TKDataRepeaterStreamItemStatus } from "@generated/Enums/Core/TKDataRepeaterStreamItemStatus";
import { RouteLocationNormalized } from "vue-router";
import { StoreUtil } from "@util/StoreUtil";

@Options({
    components: {
        FilterableListComponent,
        BackendInputComponent,
        DataRepeaterItemComponent,
        PagingComponent,
        DataRepeaterBatchActionComponent
    }
})
export default class DataRepeaterPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    @Ref() readonly filterableList!: FilterableListComponent;
    
    // Service
    service: DataRepeaterService = new DataRepeaterService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();
    metadataLoadStatus: FetchStatus = new FetchStatus();

    streamDefinitions: TKGetDataRepeaterStreamDefinitionsViewModel | null = null;
    selectedStream: TKDataRepeaterStreamViewModel | null = null;
    selectedItemId: string | null = null;
    actionParameters: any = {};
    items: Array<TKDataRepeaterStreamItemViewModel> = [];
    tagPresets: Array<string> = [];

    // Filter/pagination
    pageIndex: number = 0;
    pageSize: number = 50;
    filterItemId: string = '';
    filterTags: Array<string> = [];
    totalResultCount: number = 0;
    filterCache: any = {};
    filterRetryAllowedBinding: null | 'retryable' | 'non-retryable' = null;
    retryableChoices: Array<any> = [
        { value: null, text: 'Retryable & non-retryable' },
        { value: 'retryable', text: 'Retryable only' },
        { value: 'non-retryable', text: 'Non-retryable only' }
    ];

    batchActionsDialogVisible: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        StoreUtil.store.commit('showMenuButton', true);

        this.resetFilter();
        this.loadStreamDefinitions();

        await this.$router.isReady();
        this.routeListener = this.$router.afterEach((t, f, err) => this.onRouteChanged(t, f));
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
        return StoreUtil.store.state.globalOptions;
    }
    
    get isLoading(): boolean {
        return this.metadataLoadStatus.inProgress || this.dataLoadStatus.inProgress;
    }

    get hasBatchActions(): boolean {
        return this.hasAccessToExecuteBatchActions
            && this.selectedStream != null
            && this.selectedStream.BatchActions != null
            && this.selectedStream.BatchActions.length > 0;
    }

    get hasAccessToExecuteBatchActions(): boolean {
        return this.options.AccessOptions.indexOf("ExecuteBatchActions") != -1;
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

    get filterRetryAllowed(): boolean | null {
        if (this.filterRetryAllowedBinding === null) return null;
        else return this.filterRetryAllowedBinding === 'retryable';
    };

    ////////////////
    //  METHODS  //
    //////////////
    resetFilter(): void {
        this.pageIndex = 0;
        this.pageSize = 50;
        this.filterItemId = '';
        this.filterRetryAllowedBinding = null;
        this.tagPresets = this.selectedStream?.FilterableTags || [];
        this.filterTags = this.selectedStream?.InitiallySelectedTags || [];
    }

    loadStreamDefinitions(): void {
        this.service.GetStreamDefinitions(this.metadataLoadStatus, {
            onSuccess: (data) => this.onStreamDefinitionsRetrieved(data)
        });
    }

    onStreamDefinitionsRetrieved(data: TKGetDataRepeaterStreamDefinitionsViewModel | null): void {
        this.streamDefinitions = data;
        this.streamDefinitions?.Streams.forEach(s => {
            s.Actions.forEach(a => {
                this.actionParameters[a.Id] = [];
            });
        });

        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.streamId) || null;
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

    setActiveStream(stream: TKDataRepeaterStreamViewModel | null, updateUrl: boolean = true): void {
        if (this.isLoading) {
            return;
        }

        const prevStreamid = this.selectedStream?.Id;

        this.selectedStream = stream;
        this.selectedItemId = null;
        (this.filterableList as FilterableListComponent).setSelectedItem(stream);
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
        this.loadCurrentStreamItems(true);

        if (updateUrl && StringUtils.stringOrFirstOfArray(this.$route.params.streamId) != StringUtils.stringOrFirstOfArray(this.hash(stream.Id)))
        {
            this.$router.push(`/dataRepeater/${this.hash(stream.Id)}`);
        }
    }

    hash(input: string) { return HashUtils.md5(input); }

    loadCurrentStreamItems(resetPageIndex: boolean): void {
        if (!this.selectedStream) return;

        this.updateUrlFromFilter();
        if (resetPageIndex)
        {
            this.pageIndex = 0;
        }

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

    onStreamItemsLoaded(data: TKDataRepeaterStreamItemsPagedViewModel): void {
        this.totalResultCount = data.TotalCount;
        this.items = data.Items;
        
        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.itemId) || null;
        this.setActiveItemId(idFromHash, false);
    }

    onFilterChanged(): void {
        this.updateUrlFromFilter();
        this.loadCurrentStreamItems(true);
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
            this.filterRetryAllowedBinding = (retryAllowed == 'true' || retryAllowed == true)
                ? 'retryable' : 'non-retryable';
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
            this.filterRetryAllowedBinding = (this.filterCache[streamId].r == 'true' || this.filterCache[streamId].r == true)
                ? 'retryable' : 'non-retryable';
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

    onItemClickedMiddle(item: TKDataRepeaterStreamItemViewModel): void {
        const route = `#/dataRepeater/${this.hash(this.selectedStream?.Id||'')}/${item.Id}`;
        UrlUtils.openRouteInNewTab(route);
    }

    formatDate(date: Date): string {
        return DateUtils.FormatDate(date, "dd/MM/yy HH:mm:ss");
    }

    itemRowClasses(item: TKDataRepeaterStreamItemViewModel): any {
        let success = item.LastRetryWasSuccessful;
        let failed = !success && (item.LastRetryWasSuccessful == false || item.Error || item.FirstError);
        if (item.ForcedStatus)
        {
            success = item.ForcedStatus == TKDataRepeaterStreamItemStatus.Success;
            failed = item.ForcedStatus == TKDataRepeaterStreamItemStatus.Error;
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

    // @Watch("pageIndex")
    onPageIndexChanged(): void {
        this.loadCurrentStreamItems(false);
    }

    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
        if (!this.streamDefinitions || !to.path.toLowerCase().startsWith('/datarepeater/')) return;

        const oldStreamIdFromHash = StringUtils.stringOrFirstOfArray(from.params.streamId) || null;
        const newStreamIdFromHash = StringUtils.stringOrFirstOfArray(to.params.streamId) || null;
        const streamChanged = oldStreamIdFromHash != newStreamIdFromHash;

        const oldItemIdFromHash = StringUtils.stringOrFirstOfArray(from.params.itemId) || null;
        const newItemIdFromHash = StringUtils.stringOrFirstOfArray(to.params.itemId) || null;
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

    onItemUpdated(item: TKDataRepeaterStreamItemViewModel): void {
        const index = this.items.findIndex(x => x.Id == item.Id);
        this.items[index] = item;
    }
}
</script>

<style scoped lang="scss">
.pagination-and-actions {
}
.data-repeater-filters {
    display: flex;
    flex-direction: row;
    flex-wrap: wrap;
    align-items: flex-end;
}
.data-repeater-list-item {
    cursor: pointer;
    display: flex;
    flex-direction: row;
    flex-wrap: wrap;
    padding: 5px;
    align-items: center;
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
        margin-top: 4px;
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
        padding: 5px 6px;
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
.data-repeater-batch-action {
    border: 2px solid #d6d6d6;
    margin-bottom: 20px;
}
</style>