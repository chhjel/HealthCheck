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
                    :showFilter="false"
                    :groupIfSingleGroup="false"
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

                                <div class="data-export-filters" v-if="showQuery">
                                    <b>{{ queryTitle }}</b>
                                    <editor-component
                                        class="editor mb-2"
                                        :language="'csharp'"
                                        v-model="queryInput"
                                        :read-only="isLoading || !hasAccessToQueryCustom"
                                        ref="editor" />
                                </div>

                                <code v-if="queryError"
                                    @click="showQueryErrorDetails = !showQueryErrorDetails"
                                    class="mb-2"
                                    style="cursor: pointer">{{ queryError }}</code>
                                <code v-if="queryErrorDetails && showQueryErrorDetails"
                                    class="mb-2">{{ queryErrorDetails }}</code>
                                
                                <div class="data-export-filters" v-if="hasAccessToQueryCustom">
                                    <v-autocomplete
                                        v-model="includedProperties"
                                        :disabled="isLoading"
                                        :items="availableProperties"
                                        chips
                                        clearable
                                        :label="includedProperties.length == 0 ? 'Included properties - All' : 'Included properties'"
                                        multiple
                                        ></v-autocomplete>
                                </div>
                            
                                <div class="data-export-actions">
                                    <v-btn :disabled="isLoading" v-if="hasAccessToQueryPreset" @click="onLoadPresetsClicked">
                                        <v-icon size="20px" class="mr-2">file_upload</v-icon>Load preset..
                                    </v-btn>
                                    <v-btn :disabled="isLoading" v-if="hasAccessToSavePreset" @click="onSavePresetClicked">
                                        <v-icon size="20px" class="mr-2">save_alt</v-icon>Save preset..
                                    </v-btn>
                                    <v-btn :disabled="isLoading" v-if="showExport" @click="onShowColumnTitlesClicked"
                                        :loading="exportLoadStatus.inProgress">
                                        <v-icon size="20px" class="mr-2">title</v-icon>Column titles..
                                    </v-btn>
                                    <v-btn :disabled="isLoading" v-if="showExport" @click="onShowExportDialogClicked"
                                        :loading="exportLoadStatus.inProgress">
                                        <v-icon size="20px" class="mr-2">file_download</v-icon>Export..
                                    </v-btn>
                                    <v-btn :disabled="isLoading" @click="loadCurrentStreamItems(true)" v-if="showExecuteQuery"
                                        color="primary">
                                        <v-icon size="20px" class="mr-2">search</v-icon>Execute query
                                    </v-btn>
                                </div>

                                <paging-component
                                    :count="totalResultCount"
                                    :pageSize="pageSize"
                                    v-model="pageIndex"
                                    @change="onPageIndexChanged"
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
                                <p>{{ resultCountText }}</p>
                                <div style="clear: both"></div>
                                <div class="table-overflow-wrapper" v-if="items.length > 0">
                                    <table class="v-table theme--light">
                                        <thead>
                                            <draggable
                                                v-model="headers"
                                                group="grp"
                                                style="min-height: 10px"
                                                tag="tr"
                                                @end="onHeaderDragEnded">
                                                <template v-for="header in headers">
                                                    <th class="column text-xs-left draggable-header"
                                                        :key="`header-${header}`"
                                                        >{{ getHeaderName(header) }}</th>
                                                </template>
                                            </draggable>
                                        </thead>
                                        <tbody>
                                            <tr v-for="(item, iIndex) in items"
                                                :key="`item-row-${iIndex}`">
                                                <td v-for="header in headers"
                                                    :key="`item-row-${iIndex}-column-${header}`"
                                                    >{{ item[header] }}</td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                                
                                <paging-component
                                    :count="totalResultCount"
                                    :pageSize="pageSize"
                                    v-model="pageIndex"
                                    @change="onPageIndexChanged"
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

        <!-- DIALOGS -->
        <v-dialog v-model="loadPresetDialogVisible"
            @keydown.esc="loadPresetDialogVisible = false"
            max-width="480"
            content-class="confirm-dialog"
            :persistent="dataLoadStatus.inProgress">
            <v-card>
                <v-card-title class="headline">Select a query preset to load</v-card-title>
                <v-card-text>
                    <v-progress-linear 
                        v-if="isLoading"
                        indeterminate color="green"></v-progress-linear>
                    <!-- NO PRESETS YET -->
                    <div v-if="!presets || presets.length == 0 && !dataLoadStatus.inProgress">
                        <b>No presets created yet.</b>
                    </div>
                    <!-- HAS PRESETS -->
                    <div v-if="presets && presets.length > 0">
                        <ul>
                            <li v-for="(preset, pIndex) in presets"
                                :key="`item-d-${preset.Id}-preset-${pIndex}`">
                                <a href="#" @click="applyPreset(preset)">{{ preset.Name }}</a>
                            </li>
                            
                        </ul>
                    </div>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="loadPresetDialogVisible = false">Cancel</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog v-model="savePresetDialogVisible"
            @keydown.esc="savePresetDialogVisible = false"
            max-width="480"
            content-class="confirm-dialog"
            :persistent="true">
            <v-card>
                <v-card-title class="headline">Save preset</v-card-title>
                <v-card-text>
                    <h3>Save current query as a preset?</h3>
                    
                    <v-text-field
                        v-model="newPresetName"
                        placeholder="Preset name"
                        :disabled="dataLoadStatus.inProgress" />

                    <h4>Query</h4>
                    <code>{{ queryInput }}</code>

                    <h4 class="mt-2">Included properties</h4>
                    <code>{{ includedProperties }}</code>
                    
                    <div v-if="headerNameOverrides && Object.keys(headerNameOverrides).length > 0">
                        <h4 class="mt-2">Column title overrides</h4>
                        <div v-for="(headerOverride, hIndex) in Object.keys(headerNameOverrides)"
                            :key="`header-override-preview-${hIndex}`">
                            <code>{{ headerOverride }}</code> =&gt; <code>{{ headerNameOverrides[headerOverride] }}</code>
                        </div>
                    </div>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="savePresetDialogVisible = false">Cancel</v-btn>
                    <v-btn color="primary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="onSavePresetConfirmClicked()">Save</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog v-model="exportDialogVisible"
            @keydown.esc="exportDialogVisible = false"
            max-width="320"
            content-class="confirm-dialog"
            :persistent="exportLoadStatus.inProgress">
            <v-card>
                <v-card-title class="headline">Select export format</v-card-title>
                <v-card-text>
                    <div>
                        <ul>
                            <div v-for="(exporter, eIndex) in exporters"
                                :key="`item-d-${exporter.Id}-export-${eIndex}`">
                                <v-btn color="primary"
                                    :disabled="isLoading"
                                    :loading="exportLoadStatus.inProgress"
                                    @click="onExportClicked(exporter.Id)">
                                    <v-icon size="20px" class="mr-2">file_download</v-icon>{{ exporter.Name }}
                                </v-btn>
                            </div>
                        </ul>
                    </div>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary"
                        :disabled="exportLoadStatus.inProgress"
                        :loading="exportLoadStatus.inProgress"
                        @click="exportDialogVisible = false">Cancel</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog v-model="columnTitlesDialogVisible"
            @keydown.esc="columnTitlesDialogVisible = false"
            max-width="460"
            content-class="confirm-dialog">
            <v-card>
                <v-card-title class="headline">Customize column titles</v-card-title>
                <v-card-text>
                    <div>
                        <div v-if="!headers || Object.keys(headers).length == 0">
                            <b>No columns yet, perform a query first.</b>
                        </div>
                        <ul>
                            <div v-for="(header, hIndex) in headers"
                                :key="`item-header-override-${hIndex}`">
                                <code>{{ header }}</code>
                                <v-text-field
                                    v-model="headerNameOverrides[header]"
                                    :placeholder="header" />
                            </div>
                        </ul>
                    </div>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="columnTitlesDialogVisible = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import draggable from 'vuedraggable'
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
import EditorComponent from "components/Common/EditorComponent.vue";
import { HCDataExportStreamItemDefinitionMemberViewModel } from "generated/Models/Module/DataExport/HCDataExportStreamItemDefinitionMemberViewModel";
import { HCDataExportStreamQueryPresetViewModel } from "generated/Models/Module/DataExport/HCDataExportStreamQueryPresetViewModel";
import { HCDataExportQueryRequest } from "generated/Models/Module/DataExport/HCDataExportQueryRequest";
import { HCDataExportExporterViewModel } from "generated/Models/Module/DataExport/HCDataExportExporterViewModel";

@Component({
    components: {
        draggable,
        FilterableListComponent,
        PagingComponent,
        EditorComponent
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
    exportLoadStatus: FetchStatus = new FetchStatus();

    streamDefinitions: HCGetDataExportStreamDefinitionsViewModel | null = null;
    exporters: Array<HCDataExportExporterViewModel> = [];
    selectedStream: HCDataExportStreamViewModel | null = null;
    selectedItemId: string | null = null;
    items: Array<any> = [];
    selectedPresetId: string | null = null;
    queryError: string | null = null;
    queryErrorDetails: string | null = null;
    showQueryErrorDetails: boolean = false;
    includedProperties: Array<string> = [];
    availableProperties: Array<string> = [];
    lastQueriedProperties: Array<string> = [];
    headers: Array<string> = [];
    headerNameOverrides: { [key:string]: string } = {};
    itemDefinition: HCDataExportStreamItemDefinitionViewModel = { StreamId: '', Name: '', Members: [] };
    
    newPresetName: string = '';
    loadPresetDialogVisible: boolean = false;
    savePresetDialogVisible: boolean = false;
    presets: Array<HCDataExportStreamQueryPresetViewModel> = [];
    exportDialogVisible: boolean = false;
    columnTitlesDialogVisible: boolean = false;

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

        this.refreshEditorSize();
        this.$nextTick(() => this.refreshEditorSize());
        setTimeout(() => {
            this.refreshEditorSize();
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

    get hasAccessToQueryCustom(): boolean {
        return this.hasAccess('QueryCustom');
    }

    get hasAccessToQueryPreset(): boolean {
        return this.hasAccess('QueryPreset') 
            && this.streamDefinitions?.SupportsStorage == true
            && this.hasAccessToLoadPreset;
    }

    get hasAccessToSavePreset(): boolean {
        return this.hasAccess('SavePreset') && this.streamDefinitions?.SupportsStorage == true;
    }

    get hasAccessToLoadPreset(): boolean {
        return this.hasAccess('LoadPreset') && this.streamDefinitions?.SupportsStorage == true;
    }

    get hasAccessToDeletePreset(): boolean {
        return this.hasAccess('DeletePreset') && this.streamDefinitions?.SupportsStorage == true;
    }

    get hasAccessToExport(): boolean {
        return this.hasAccess('Export');
    }

    get showExecuteQuery(): boolean {
        return this.hasAccessToQueryCustom || this.selectedPresetId != null;
    }

    get showExport(): boolean {
        return this.hasAccessToExport 
            && (this.selectedPresetId != null || this.hasAccessToQueryCustom)
            && this.exporters.length > 0;
    }

    get showQuery(): boolean {
        return this.hasAccessToQueryCustom || this.selectedPresetId != null;
    }

    get queryTitle(): string {
        if (this.hasAccessToQueryCustom) return 'Query';
        else if (!this.selectedPresetId) return 'Query';
        const preset = this.presets.filter(x => x.Id == this.selectedPresetId)[0];
        if (!preset) return 'Query';
        return preset.Name || 'Query';
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

    get resultCountText(): string {
        let from = (this.pageIndex * this.pageSize)+1;
        from = Math.min(from, this.totalResultCount);
        let to = (this.pageIndex * this.pageSize) + this.items.length;
        return `Showing ${from}-${to} of ${this.totalResultCount} total matches`;
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
    hasAccess(option: string): boolean {
        return this.options.AccessOptions.indexOf(option) != -1;
    }

    refreshEditorSize(): void {
        const editor: EditorComponent = <EditorComponent>this.$refs.editor;
        if (editor)
        {
            editor.refreshSize();
        }
    }

    resetFilter(): void {
        this.pageIndex = 0;
        this.pageSize = 50;
        this.queryInput = '';
        this.includedProperties = [];
        this.selectedPresetId = null;
        this.headers = [];
        this.headerNameOverrides = {};
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
            this.exporters = this.streamDefinitions.Exporters;
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
        this.items = [];
        this.totalResultCount = 0;
        if (this.applyFilterFromCache(stream.Id))
        {
            this.$nextTick(() => this.updateUrlFromFilter());
        } else {
            this.applyFilterFromUrl();
        }
        // this.loadCurrentStreamItems();

        if (updateUrl && this.$route.params.streamId != this.hash(stream.Id))
        {
            this.$router.push(`/dataExport/${this.hash(stream.Id)}`);
        }

        this.$nextTick(() => {
            if (!this.hasAccessToQueryCustom && this.hasAccessToQueryPreset && !this.loadPresetDialogVisible)
            {
                this.onLoadPresetsClicked();
            }
        });
    }

    getHeaderName(name: string): string {
        return this.headerNameOverrides[name] || name;
    }

    hash(input: string) { return HashUtils.md5(input); }

    loadPresets(): void {
        this.service.GetStreamQueryPresets(this.selectedStream?.Id || '', this.dataLoadStatus, {
            onSuccess: (data) => {
                if (data)
                {
                    this.presets = data;
                }
            }
        });
    }

    applyPreset(preset: HCDataExportStreamQueryPresetViewModel): void {
        this.items = [];
        this.totalResultCount = 0;
        this.selectedPresetId = preset.Id;
        this.newPresetName = preset.Name;
        this.queryInput = preset.Query;
        this.includedProperties = [...preset.IncludedProperties];
        this.headers = [...preset.IncludedProperties];
        this.headerNameOverrides = JSON.parse(JSON.stringify(preset.HeaderNameOverrides));
        this.loadPresetDialogVisible = false;
    }

    deletePreset(id: string): void {
        this.service.DeleteStreamQueryPreset({
            StreamId: this.selectedStream?.Id || '',
            Id: id
        }, this.dataLoadStatus,
        {
            onSuccess: () => {
                this.presets = this.presets.filter(x => x.Id != id);
            }
        });
    }

    savePreset(name: string, existingPreset: HCDataExportStreamQueryPresetViewModel | null = null): void {
        let props = this.includedProperties;
        this.headers.forEach(x => {
            if (props.includes(x))
            {
                props = props.filter(p => p != x);
                props.unshift(x);
            }
        });
        props = props.reverse();

        this.service.SaveStreamQueryPreset({
            StreamId: this.selectedStream?.Id || '',
            Preset: {
                Id: existingPreset?.Id || '00000000-0000-0000-0000-000000000000',
                Name: name,
                Description: '',
                Query: this.queryInput,
                IncludedProperties: props,
                HeaderNameOverrides: this.headerNameOverrides
            }
        }, this.dataLoadStatus,
        {
            onSuccess: (d) => {
                this.savePresetDialogVisible = false;
            }
        });
    }

    loadCurrentStreamItems(resetPageIndex: boolean): void {
        if (!this.selectedStream) return;

        this.updateUrlFromFilter();
        if (resetPageIndex)
        {
            this.pageIndex = 0;
        }

        this.lastQueriedProperties = this.includedProperties;

        this.service.QueryStreamPaged(this.createQueryPayload(), this.dataLoadStatus, {
            onSuccess: (data) => {
                if (data != null)
                {
                    this.onStreamItemsLoaded(data);
                }
            }
        })
    }

    createQueryPayload(exporterId: string | null = null): HCDataExportQueryRequest {
        if (this.includedProperties.length == 0)
        {
            this.includedProperties = this.availableProperties.filter(x => !this.availableProperties.some(a => a.startsWith(`${x}.`)));
        }

        this.includedProperties.forEach(x => {
            if (!this.headers.includes(x))
            {
                this.headers.push(x);
            }
        });
        this.headers = this.headers.filter(x => this.includedProperties.includes(x));

        return {
            StreamId: this.selectedStream?.Id || '',
            PageIndex: this.pageIndex,
            PageSize: this.pageSize,
            Query: this.hasAccessToQueryCustom ? this.queryInput : '',
            IncludedProperties: this.hasAccessToQueryCustom ? this.headers : [],
            PresetId: this.hasAccessToQueryCustom ? undefined : (this.selectedPresetId || undefined),
            HeaderNameOverrides: this.hasAccessToQueryCustom ? this.headerNameOverrides : {},
            ExporterId: exporterId || ''
        };
    }

    onStreamItemsLoaded(data: HCDataExportQueryResponseViewModel): void {
        this.totalResultCount = data.TotalCount;
        this.items = data.Items;

        this.queryError = data.ErrorMessage;
        this.queryErrorDetails = data.ErrorDetails;
    }

    onFilterChanged(loadData: boolean, resetPageIndex: boolean): void {
        this.updateUrlFromFilter();

        if (loadData)
        {
            this.loadCurrentStreamItems(resetPageIndex);
        }
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
            q: this.queryInput,
            pId: this.selectedPresetId,
            incProp: this.includedProperties,
            headers: this.headers,
            headerOverrides: this.headerNameOverrides
        };
    }

    applyFilterFromCache(streamId: string): boolean {
        const cache = this.filterCache[streamId];
        if (cache == null) return false;
        
        this.queryInput = this.filterCache[streamId].q || '';
        this.selectedPresetId =  this.filterCache[streamId].pId || null;
        this.includedProperties =  this.filterCache[streamId].incProp || [];
        this.headers =  this.filterCache[streamId].headers || [];
        this.headerNameOverrides =  this.filterCache[streamId].headerOverrides || {};

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

    // @Watch("pageIndex")
    onPageIndexChanged(): void {
        this.loadCurrentStreamItems(false);
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

    onHeaderDragEnded(): void {

    }

    onLoadPresetsClicked(): void {
        this.loadPresetDialogVisible = true;
        this.loadPresets();
    }
    
    onSavePresetClicked(): void {
        this.savePresetDialogVisible = true;
    }

    onSavePresetConfirmClicked(): void {
        this.savePreset(this.newPresetName, null);
    }

    onShowExportDialogClicked(): void {
        this.exportDialogVisible = true;
    }

    onExportClicked(exporterId: string): void {
        this.service.PrepareExport(this.createQueryPayload(exporterId), this.exportLoadStatus,
        {
            onSuccess: (key) => {
                if (key)
                {
                    const url = this.service.CreateExportDownloadUrl(this.globalOptions.EndpointBase, key);
                    window.open(url, '_blank');
                    this.exportDialogVisible = false;
                }
            }
        });
    }

    onShowColumnTitlesClicked(): void {
        this.columnTitlesDialogVisible = true;
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
.table-overflow-wrapper {
    overflow-x: auto;
}
.draggable-header {
    cursor: grab;
}
.editor {
  width: 100%;
  height: 88px;
  border: 1px solid #949494;
}
</style>
