<!-- src/components/modules/DataExport/DataExportPageComponent.vue -->
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
                                <h2 v-if="selectedStream.Name">{{ selectedStream.Name }}</h2>
                                <p v-if="selectedStream.Description" v-html="selectedStream.Description"></p>

                                <div class="data-export-filters">
                                    <v-text-field
                                        v-model="queryInput"
                                        @keyup.enter="onFilterChanged"
                                        label="Query"
                                        clearable
                                        class="filter-input"
                                        :readonly="isLoading"
                                    ></v-text-field>
                                </div>

                                <code v-if="queryError"
                                    @click="showQueryErrorDetails = !showQueryErrorDetails"
                                    class="mb-2"
                                    style="cursor: pointer">{{ queryError }}</code>
                                <code v-if="queryErrorDetails && showQueryErrorDetails"
                                    class="mb-2">{{ queryErrorDetails }}</code>
                                
                                <div class="data-export-filters">
                                    <v-select
                                        v-model="includedProperties"
                                        :items="availableProperties"
                                        label="Included properties"
                                        multiple
                                        chips
                                        clearable
                                        :readonly="isLoading"
                                        ></v-select>
                                </div>
                            
                                <v-btn @click="loadCurrentStreamItems" :disabled="isLoading" class="right">
                                    <v-icon size="20px" class="mr-2">refresh</v-icon>Query
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
                                        :key="`item-${iIndex}`"
                                        class="data-export-list-item"
                                        tabindex="0">
                                        {{ item }}
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
import DataExportService from  '../../../services/DataExportService';
import { HCDataExportStreamViewModel } from "generated/Models/Module/DataExport/HCDataExportStreamViewModel";
import PagingComponent from "../../Common/Basic/PagingComponent.vue";
import HashUtils from "../../../util/HashUtils";
import { Route } from "vue-router";
import DateUtils from "util/DateUtils";
import UrlUtils from "util/UrlUtils";
import { HCGetDataExportStreamDefinitionsViewModel } from "generated/Models/Module/DataExport/HCGetDataExportStreamDefinitionsViewModel";
import { HCDataExportQueryResponseViewModel } from "generated/Models/Module/DataExport/HCDataExportQueryResponseViewModel";
import { HCDataExportStreamItemDefinitionViewModel } from "generated/Models/Module/DataExport/HCDataExportStreamItemDefinitionViewModel";

@Component({
    components: {
        FilterableListComponent,
        PagingComponent
    }
})
export default class DataRepeaterPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    // Service
    service: DataExportService = new DataExportService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();
    metadataLoadStatus: FetchStatus = new FetchStatus();

    streamDefinitions: HCGetDataExportStreamDefinitionsViewModel | null = null;
    selectedStream: HCDataExportStreamViewModel | null = null;
    selectedItemId: string | null = null;
    items: Array<any> = [];
    queryError: string | null = null;
    queryErrorDetails: string | null = null;
    showQueryErrorDetails: boolean = false;
    includedProperties: Array<string> = [];
    availableProperties: Array<string> = [];
    itemDefinition: HCDataExportStreamItemDefinitionViewModel = { StreamId: '', Name: '', Members: [] };

    // Filter/pagination
    pageIndex: number = 0;
    pageSize: number = 50;
    queryInput: string = '';
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
        this.queryInput = '';
    }

    loadStreamDefinitions(): void {
        this.service.GetStreamDefinitions(this.metadataLoadStatus, {
            onSuccess: (data) => this.onStreamDefinitionsRetrieved(data)
        });
    }

    onStreamDefinitionsRetrieved(data: HCGetDataExportStreamDefinitionsViewModel | null): void {
        this.streamDefinitions = data;

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

    setActiveStream(stream: HCDataExportStreamViewModel | null, updateUrl: boolean = true): void {
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
        
        this.availableProperties = stream.ItemDefinition.Members.map(x => x.Name);
        this.itemDefinition = stream.ItemDefinition;

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
            this.$router.push(`/dataExport/${this.hash(stream.Id)}`);
        }
    }

    hash(input: string) { return HashUtils.md5(input); }

    loadCurrentStreamItems(): void {
        if (!this.selectedStream) return;

        this.service.QueryStreamPaged({
            StreamId: this.selectedStream.Id,
            PageIndex: this.pageIndex,
            PageSize: this.pageSize,
            Query: this.queryInput,
            IncludedProperties: this.includedProperties
        }, this.dataLoadStatus, {
            onSuccess: (data) => {
                if (data != null)
                {
                    this.onStreamItemsLoaded(data);
                }
            }
        })
    }

    onStreamItemsLoaded(data: HCDataExportQueryResponseViewModel): void {
        this.totalResultCount = data.TotalCount;
        this.items = data.Items;

        this.queryError = data.ErrorMessage;
        this.queryErrorDetails = data.ErrorDetails;
    }

    onFilterChanged(): void {
        this.updateUrlFromFilter();
        this.loadCurrentStreamItems();
    }

    updateUrlFromFilter(): void {
        let query: any = {};

        if (this.queryInput != '')
        {
            query.q = this.queryInput;
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
            this.queryInput = this.$route.query.q as string || '';
        }
    }

    cacheFilter(streamId: string): void {
        this.filterCache[streamId] = {
            q: this.queryInput
        };
    }

    applyFilterFromCache(streamId: string): boolean {
        const cache = this.filterCache[streamId];
        if (cache == null) return false;
        
        this.queryInput = this.filterCache[streamId].q || '';

        return true;
    }
    
    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onMenuItemClicked(item: FilterableListItem): void {
        this.setActiveStream(item.data);
    }

    onMenuItemMiddleClicked(item: FilterableListItem): void {
        if (item && item.data && item.data.Id)
        {
            const idHash = this.hash(item.data.Id);
            const route = `#/dataExport/${idHash}`;
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

        if (streamChanged)
        {
            const matchingStream = this.streamDefinitions.Streams.filter(x => this.hash(x.Id) == newStreamIdFromHash)[0] || null;
            this.setActiveStream(matchingStream, false);
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
.data-export-filters {
    display: flex;
    flex-direction: row;
    flex-wrap: wrap;
    align-items: baseline;
}
.data-export-list-item {

}
</style>