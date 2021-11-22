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
                    :filterKeys="[ 'Name', 'Description' ]"
                    :loading="metadataLoadStatus.inProgress"
                    :disabled="dataLoadStatus.inProgress"
                    ref="filterableList"
                    v-on:itemClicked="onMenuItemClicked"
                    />
            </v-navigation-drawer>
            
            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root">
                <v-layout>
                    <v-flex>
                        <v-container>
                            <div v-if="selectedStream && selectedItemId == null">
                                <h2 v-if="selectedStream.StreamItemsName">{{ selectedStream.StreamItemsName }}</h2>

                                <div class="data-repeater-filters">
                                    <v-text-field
                                        v-model="filterItemId"
                                        @blur="loadCurrentStreamItems"
                                        @keyup.enter="loadCurrentStreamItems"
                                        label="Filter"
                                        clearable
                                        class="filter-input"
                                    ></v-text-field>
                                    <v-combobox
                                        v-model="filterTags"
                                        @blur="loadCurrentStreamItems"
                                        @keyup.enter="loadCurrentStreamItems"
                                        :items="detectedTags"
                                        label="Included tags"
                                        multiple
                                        chips
                                        class="filter-input"
                                        ></v-combobox>
                                </div>

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
                                v-if="dataLoadStatus.inProgress"
                                indeterminate color="green"></v-progress-linear>

                            <!-- DATA LOAD ERROR -->
                            <v-alert :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
                            {{ dataLoadStatus.errorMessage }}
                            </v-alert>

                            <div v-if="selectedStream && selectedItemId == null">
                                <div v-for="(item, iIndex) in items"
                                    :key="`item-${iIndex}-${item.Id}`"
                                    @click="setActiveItemId(item.Id)"
                                    @keyup.enter="setActiveItemId(item.Id)"
                                    class="data-repeater-list-item"
                                    tabindex="0">
                                    <span class="data-repeater-list-item--title">{{ selectedStream.ItemIdName }}: {{ item.ItemId }}</span>
                                    <span v-if="item.Summary" class="data-repeater-list-item--summary">{{ item.Summary }}</span>
                                    <div class="data-repeater-list-item--tags" v-if="item.Tags && item.Tags.length > 0">
                                        <div class="data-repeater-list-item--tag">{{ item.Tags.join(', ') }}</div>
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
    actionParameters: any = {}; //Array<string> = [];
    items: Array<HCDataRepeaterStreamItemViewModel> = [];

    // Filter/pagination
    pageIndex: number = 0;
    pageSize: number = 50;
    filterItemId: string = '';
    filterTags: Array<string> = [];
    totalResultCount: number = 0;

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
    
    get menuItems(): Array<FilterableListItem>
    {
        if (!this.streamDefinitions) return [];
        return this.streamDefinitions.Streams.map(x => {
            return {
                title: x.Name,
                subtitle: '',
                data: x
            };
        });
    }

    get detectedTags(): Array<string>
    {
        if (!this.items) return [];
        let tags: Set<string> = new Set<string>();
        this.items.forEach(x => {
            x.Tags.forEach(t => {
                tags.add(t);
            })
        });
        return Array.from(tags);
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
        if (this.dataLoadStatus.inProgress) {
            return;
        }

        this.selectedStream = stream;
        this.selectedItemId = null;
        if (stream == null)
        {
            return;
        }

        this.resetFilter();
        this.loadCurrentStreamItems();

        if (updateUrl)
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
            Tags: this.filterTags
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
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMenuItemClicked(item: FilterableListItem): void {
        this.setActiveStream(item.data);
    }

    @Watch("pageIndex")
    onPageIndexChanged(): void {
        this.loadCurrentStreamItems();
    }

    onRouteChanged(to: Route, from: Route): void {
        if (!this.streamDefinitions) return;

        const currentStreamId = !!this.selectedStream ? this.hash(this.selectedStream.Id) : '';

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
}
.data-repeater-list-item {
    cursor: pointer;
    display: flex;
    flex-direction: row;
    flex-wrap: wrap;
    padding: 5px;
    align-items: baseline;
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
    }
    &:focus, :active {
        background-color: #d5d5d5;
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
}
</style>