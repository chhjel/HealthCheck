<!-- src/components/modules/DataExport/DataExportPageComponent.vue -->
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
                    <h2 v-if="selectedStream.Name">{{ selectedStream.Name }}</h2>
                    <p v-if="selectedStream.Description" v-html="selectedStream.Description"></p>

                    <!-- QUERY INPUT -->
                    <div class="data-export-filters" v-if="showQuery">
                        <div style="display: flex; width: 100%">
                            <b>{{ queryTitle }}</b>
                                                                    <a href="#" v-if="hasAccessToQueryCustom" style="font-size: 13px;"
                                @click.prevent="onQueryHelpClicked">Query help</a>
                        </div>

                        <editor-component
                            class="editor mb-2"
                            :language="'csharp'"
                            v-model:value="queryInput"
                            :read-only="isLoading || !hasAccessToQueryCustom"
                            ref="editor" />
                    </div>
                    <!-- CUSTOM PARAMETERS -->
                    <div v-if="showCustomInputs">
                        <div style="display: flex; width: 100%" class="mb-2">
                            <b>{{ queryTitle }}</b>
                        </div>

                        <div class="export-parameter-items">
                            <backend-input-component
                                v-for="(parameterDef, pIndex) in selectedStream.CustomParameterDefinitions"
                                :key="`export-parameter-item-${selectedStream.Id}-${pIndex}`"
                                class="export-parameter-item"
                                v-model:value="customParameters[parameterDef.Id]"
                                :config="parameterDef"
                                :readonly="isLoading || !hasAccessToQueryCustom"
                                />
                        </div>
                    </div>

                    <code v-if="queryError"
                        @click="showQueryErrorDetails = !showQueryErrorDetails"
                        class="mb-2"
                        style="cursor: pointer">{{ queryError }}</code>
                    <code v-if="queryErrorDetails && showQueryErrorDetails"
                        class="mb-2">{{ queryErrorDetails }}</code>
                    
                    <div class="data-export-filters" v-if="hasAccessToQueryCustom">
                        <select-component
                            v-model:value="includedProperties"
                            :disabled="isLoading"
                            :items="availableProperties"
                            clearable
                            :label="includedProperties.length == 0 ? 'Included properties - All' : 'Included properties'"
                            multiple
                            allowInput
                            ></select-component>
                    </div>
                
                    <div class="data-export-actions" v-if="!isSimpleExportMode">
                        <btn-component :disabled="isLoading" v-if="hasAccessToQueryPreset || hasAccessToExport" @click="onLoadPresetsClicked">
                            <icon-component size="20px" class="mr-2">file_upload</icon-component>Load preset..
                        </btn-component>
                        <btn-component :disabled="isLoading" v-if="hasAccessToSavePreset" @click="onSavePresetClicked">
                            <icon-component size="20px" class="mr-2">save_alt</icon-component>Save preset..
                        </btn-component>
                        <btn-component :disabled="isLoading" v-if="hasAccessToQueryCustom" @click="onShowColumnTitlesClicked"
                            :loading="exportLoadStatus.inProgress">
                            <icon-component size="20px" class="mr-2">title</icon-component>Column config..
                        </btn-component>
                        <btn-component :disabled="isLoading" v-if="showExport" @click="onShowExportDialogClicked"
                            :loading="exportLoadStatus.inProgress">
                            <icon-component size="20px" class="mr-2">file_download</icon-component>Export..
                        </btn-component>
                        <btn-component :disabled="isLoading" @click="loadCurrentStreamItems(true)" v-if="showExecuteQuery"
                            color="primary">
                            <icon-component size="20px" class="mr-2">search</icon-component>Execute query
                        </btn-component>
                    </div>

                    <div v-if="isSimpleExportMode">
                        <!-- SIMPLE MODE -->
                        <progress-linear-component v-if="isLoading" indeterminate color="success"></progress-linear-component>

                        <!-- NO PRESETS YET -->
                        <div v-if="!isLoading && (!presets || presets.length == 0)">
                            <b>No export presets available, someone with access must create them first.</b>
                        </div>

                        <!-- HAS PRESETS -->
                        <div v-if="!isLoading && presets && presets.length > 0">
                            <div>
                                <h3>1. Select preset to export:</h3>
                                <div class="simple-export-buttons">
                                    <btn-component v-for="preset in presets"
                                        :key="`item-d-${preset.Id}-preset`"
                                        :class="{ 'selected': (selectedPresetId == preset.Id) }"
                                        @click.prevent="applyPreset(preset)"
                                        depressed :color="(selectedPresetId == preset.Id) ? 'primary' : '#ddd'">
                                        {{ preset.Name }}
                                    </btn-component>
                                </div>
                            </div>
                            <div>
                                <h3>2. Select export format:</h3>
                                <div class="simple-export-buttons">
                                    <div v-for="(exporter, eIndex) in exporters"
                                        :key="`item-d-${exporter.Id}-export-${eIndex}`">
                                        <btn-component color="primary"
                                            :disabled="isLoading || selectedPresetId == null"
                                            :loading="exportLoadStatus.inProgress"
                                            @click="onExportClicked(exporter.Id)"
                                            class="exporter-button">
                                            <div class="exporter-button-content">
                                                <icon-component size="20px" class="exporter-button-icon mr-2">file_download</icon-component>
                                                <div class="exporter-button-texts">
                                                    <div class="exporter-button-title">{{ exporter.Name }}</div>
                                                    <div v-if="exporter.Description" class="exporter-description">{{ exporter.Description }}</div>
                                                </div>
                                            </div>
                                        </btn-component>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- END OF SIMPLE MODE -->
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

                <!-- LOAD PROGRESS -->
                <progress-linear-component 
                    v-if="isLoading"
                    indeterminate color="success"></progress-linear-component>

                <!-- DATA LOAD ERROR -->
                <alert-component :value="dataLoadStatus.failed" v-if="dataLoadStatus.failed" type="error">
                {{ dataLoadStatus.errorMessage }}
                </alert-component>

                <div v-if="selectedStream && selectedItemId == null">
                    <p v-if="hasQueriedAtLeastOnce">{{ resultCountText }}</p>
                    <div style="clear: both"></div>
                    <div class="table-overflow-wrapper" v-if="items.length > 0">
                        <table class="v-table theme--light">
                            <thead>
                                <draggable
                                    v-model="headers"
                                    :item-key="x => `header-${x}`"
                                    group="grp"
                                    style="min-height: 10px"
                                    tag="tr"
                                    @end="onHeaderDragEnded">
                                    <template #item="{element}">
                                        <th class="column text-xs-left draggable-header"
                                            >{{ getHeaderName(element) }}</th>
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
                        v-model:value="pageIndex"
                        @change="onPageIndexChanged"
                        :asIndex="true"
                        class="mb-2 mt-2"
                        />
                </div>
            </div>
          
        </div>

        <!-- DIALOGS -->
        <dialog-component v-model:value="loadPresetDialogVisible"
            @keydown.esc="loadPresetDialogVisible = false"
            max-width="480"
            content-class="confirm-dialog"
            :persistent="dataLoadStatus.inProgress">
            <div>
                <div class="headline">Select a query preset to load</div>
                <div>
                    <progress-linear-component 
                        v-if="isLoading"
                        indeterminate color="success"></progress-linear-component>
                    <!-- NO PRESETS YET -->
                    <div v-if="!presets || presets.length == 0 && !dataLoadStatus.inProgress">
                        <b>No presets created yet.</b>
                    </div>
                    <!-- HAS PRESETS -->
                    <div v-if="presets && presets.length > 0">
                        <ul>
                            <li v-for="(preset, pIndex) in presets"
                                :key="`item-d-${preset.Id}-preset-${pIndex}`">
                                <a href="#" @click.prevent="onDeletePresetClicked(preset)"
                                    class="error--text right"
                                    v-if="hasAccessToDeletePreset">[delete]</a>
                                <a href="#" @click.prevent="applyPreset(preset)">{{ preset.Name }}</a>
                            </li>
                        </ul>
                    </div>
                </div>
                                <div>
                                        <btn-component color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="loadPresetDialogVisible = false">Cancel</btn-component>
                </div>
            </div>
        </dialog-component>
        <dialog-component v-model:value="savePresetDialogVisible"
            @keydown.esc="savePresetDialogVisible = false"
            max-width="480"
            content-class="confirm-dialog"
            :persistent="true">
            <div>
                <div class="headline">Save preset</div>
                <div>
                    <h3>Save current query as a preset?</h3>
                    
                    <text-field-component
                        v-model:value="newPresetName"
                        placeholder="Preset name"
                        :disabled="dataLoadStatus.inProgress" />

                    <h4 v-if="showQuery">Query</h4>
                    <code v-if="showQuery">{{ queryInput }}</code>

                    <div v-if="showCustomInputs && Object.keys(customParameters).length > 0">
                        <h4 class="mt-2">Filter</h4>
                        <div v-for="(key, hIndex) in Object.keys(customParameters)"
                            :key="`custom-input-preview-${hIndex}`">
                            <span>{{ key }}</span>: <code>{{ customParameters[key] }}</code>
                        </div>
                    </div>

                    <h4 class="mt-2">Included properties</h4>
                    <ul>
                        <li v-for="(prop, hIndex) in includedProperties"
                            :key="`included-prop-preview-${hIndex}`">
                            {{ prop }}
                        </li>
                    </ul>

                    <div v-if="Object.keys(customColumns).length > 0">
                        <h4 class="mt-2">Custom columns</h4>
                        <div v-for="(key, hIndex) in Object.keys(customColumns)"
                            :key="`custom-col-preview-${hIndex}`">
                            <span>{{ key }}</span>: <code>{{ customColumns[key] }}</code>
                        </div>
                    </div>
                    
                    <div v-if="hasAnyHeaderOverrides">
                        <h4 class="mt-2">Column name overrides</h4>
                        <div v-for="(headerOverride, hIndex) in Object.keys(visibleHeaderNameOverrides)"
                            :key="`header-override-preview-${hIndex}`">
                            <code>{{ headerOverride }}</code> =&gt; <code>{{ headerNameOverrides[headerOverride] }}</code>
                        </div>
                    </div>
                </div>
                                <div>
                                        <btn-component color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="savePresetDialogVisible = false">Cancel</btn-component>
                    <btn-component color="primary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="onSavePresetConfirmClicked()">Save</btn-component>
                </div>
            </div>
        </dialog-component>
        <dialog-component v-model:value="exportDialogVisible"
            @keydown.esc="exportDialogVisible = false"
            max-width="480"
            content-class="confirm-dialog"
            :persistent="exportLoadStatus.inProgress">
            <div>
                <div class="headline">Select export format</div>
                <div>
                    <div>
                        <ul>
                            <div v-for="(exporter, eIndex) in exporters"
                                :key="`item-d-${exporter.Id}-export-${eIndex}`">
                                <btn-component color="primary"
                                    :disabled="isLoading"
                                    :loading="exportLoadStatus.inProgress"
                                    @click="onExportClicked(exporter.Id)"
                                    class="exporter-button">
                                    <div class="exporter-button-content">
                                        <icon-component size="20px" class="exporter-button-icon mr-2">file_download</icon-component>
                                        <div class="exporter-button-texts">
                                            <div class="exporter-button-title">{{ exporter.Name }}</div>
                                            <div v-if="exporter.Description" class="exporter-description">{{ exporter.Description }}</div>
                                        </div>
                                    </div>
                                </btn-component>
                            </div>
                        </ul>
                    </div>
                </div>
                                <div>
                                        <btn-component color="secondary"
                        :disabled="exportLoadStatus.inProgress"
                        :loading="exportLoadStatus.inProgress"
                        @click="exportDialogVisible = false">Cancel</btn-component>
                </div>
            </div>
        </dialog-component>
        <dialog-component v-model:value="columnConfigDialogVisible"
            @keydown.esc="columnConfigDialogVisible = false"
            max-width="460"
            content-class="confirm-dialog">
            <div>
                <div class="headline">Customize columns</div>
                <div>
                    <div>
                        <div v-if="!headers || Object.keys(headers).length == 0">
                            <b>No columns yet, perform a query first.</b>
                        </div>
                        <ul>
                            <div v-for="(header, hIndex) in headers"
                                :key="`item-header-override-${hIndex}`"
                                class="item-header-override-config">
                                <h4>{{ header }}</h4>
                                <div class="item-header-override-config__row">
                                    <text-field-component
                                        label="Name override"
                                        v-model:value="headerNameOverrides[header]"
                                        :placeholder="header" />
                                    <btn-component flat
                                        v-if="hasFormatterForHeader(header)"
                                        @click="onValueFormatButtonClicked(header)">Format</btn-component>
                                    <btn-component flat color="error"
                                        v-if="isCustomHeader(header)"
                                        @click="onRemoveCustomHeaderButtonClicked(header)">Remove</btn-component>
                                </div>
                                <div class="item-header-override-config__row">
                                    <text-field-component
                                        label="Custom value"
                                        v-if="isCustomHeader(header)"
                                        v-model:value="customColumns[header]"
                                        append-icon="insert_link"
                                        @click:append="onShowPlaceholdersClicked(header)" />
                                </div>
                            </div>
                        </ul>
                    </div>
                </div>
                                <div>
                    <btn-component color="primary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="onAddCustomHeaderButtonClicked">Add custom column</btn-component>
                                        <btn-component color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="columnConfigDialogVisible = false">Close</btn-component>
                </div>
            </div>
        </dialog-component>
        <dialog-component v-model:value="queryHelpDialogVisible"
            @keydown.esc="queryHelpDialogVisible = false"
            max-width="720"
            content-class="confirm-dialog">
            <div>
                <div class="headline">Query help</div>
                <div>
                    <div>
                        <p>
                            The query is using LINQ with a few additions. Instead of <code>&amp;&amp;</code> or <code>||</code> you can use <code>and</code> or <code>or</code>.
                        </p>

                        <h4>Simple predicates</h4>
                        <code>Name == "Jimmy" &amp;&amp; Age > 50</code><br />
                        <code>(Name == "Jimmy" and Age > 50) or Name == "Smithy"</code>

                        <h4 class="mt-2">Methods</h4>
                        <p class="mb-0">The usual methods can be used, e.g. ToString(), StartsWith(), Contains() etc.</p>
                        <code>SomeNumber.ToString().StartsWith("8")</code>

                        <h4 class="mt-2">Dates</h4>
                        <p class="mb-0">If there's a date property available it can be compared to DateTime.Now to e.g. get the last weeks data:</p>
                        <code>Created &gt; DateTime.Now.AddDays(-7)</code>

                        <h4 class="mt-2">Null Propagation</h4>
                        <p class="mb-0">To filter on properties that can be null, use <code>np()</code> instead of <code>?.</code> Or just nullcheck it first.</p>
                        <code>np(Address.City) == "DevTown"</code><br />
                        <code>Address != null &amp;&amp; Address.City == "DevTown"</code>
                    </div>
                </div>
                                <div>
                                        <btn-component color="secondary" @click="queryHelpDialogVisible = false">Close</btn-component>
                </div>
            </div>
        </dialog-component>
        <dialog-component v-model:value="deletePresetDialogVisible"
            @keydown.esc="deletePresetDialogVisible = false"
            max-width="480"
            content-class="confirm-dialog"
            :persistent="deleteLoadStatus.inProgress">
            <div>
                <div class="headline">Delete preset?</div>
                <div>
                    Delete preset <code v-if="presetToDelete">{{ presetToDelete.Name }}</code>?
                </div>
                                <div>
                                        <btn-component color="secondary"
                        :disabled="deleteLoadStatus.inProgress"
                        :loading="deleteLoadStatus.inProgress"
                        @click="deletePresetDialogVisible = false">Cancel</btn-component>
                    <btn-component color="error"
                        :disabled="deleteLoadStatus.inProgress"
                        :loading="deleteLoadStatus.inProgress"
                        @click="onDeletePresetConfirmed()">Delete</btn-component>
                </div>
            </div>
        </dialog-component>
        <dialog-component v-model:value="formatDialogVisible"
            @keydown.esc="formatDialogVisible = false"
            max-width="460"
            content-class="confirm-dialog">
            <div>
                <div class="headline">Format column</div>
                <div>
                    <div>
                        <div v-if="!formattersInDialog || formattersInDialog.length == 0">
                            <b>No formatters for this column type found.</b>
                        </div>
                        <div v-else>
                            <select-component
                                label="Selected formatter"
                                v-model:value="selectedFormatterId"
                                :items="formattersInDialogChoices"
                                item-text="Name" item-value="Id"
                                :disabled="dataLoadStatus.inProgress"
                                v-on:change="onFormatterChanged"
                                >
                            </select-component>
                            
                            <div v-if="selectedFormatter">
                                <h3>{{ selectedFormatter.Name }}</h3>
                                <p v-if="selectedFormatter.Description">{{ selectedFormatter.Description }}</p>

                                <div class="format-parameter-items"
                                    v-if="valueFormatterConfigs[selectedFormatHeader].CustomParameters">
                                    <backend-input-component
                                        v-for="(parameterDef, pIndex) in selectedFormatter.CustomParameterDefinitions"
                                        :key="`format-parameter-item-${selectedFormatter.Id}-${pIndex}-${parameterDef.Id}`"
                                        class="format-parameter-item"
                                        v-model:value="valueFormatterConfigs[selectedFormatHeader].CustomParameters[parameterDef.Id]"
                                        :config="parameterDef"
                                        :readonly="isLoading"
                                        />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                                <div>
                                        <btn-component color="secondary"
                        :disabled="dataLoadStatus.inProgress"
                        :loading="dataLoadStatus.inProgress"
                        @click="formatDialogVisible = false">Close</btn-component>
                </div>
            </div>
        </dialog-component>
        <dialog-component v-model:value="placeholdersDialogVisible"
            @keydown.esc="placeholdersDialogVisible = false"
            scrollable
            content-class="possible-placeholders-dialog">
            <div>
                <div class="headline">Select placeholder to add</div>
                                <div style="max-height: 500px;">
                    <div class="possible-placeholders-list">
                        <div v-for="(placeholder, placeholderIndex) in availableProperties"
                            :key="`possible-placeholder-${placeholderIndex}`"
                            @click="onAddPlaceholderClicked(placeholder)"
                            class="possible-placeholder-list-item">
                            <div>
                                <icon-component>add</icon-component>
                            </div>

                            <div>
                                <div class="possible-placeholder-item-title">{{ `\{${placeholder}\}` }}</div>
                            </div>
                        </div>
                    </div>
                </div>
                                <div>
                                        <btn-component color="secondary" flat @click="placeholdersDialogVisible = false">Close</btn-component>
                </div>
            </div>
        </dialog-component>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import draggable from 'vuedraggable'
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import FilterableListComponent from '@components/Common/FilterableListComponent.vue';
import { FilterableListItem } from '@components/Common/FilterableListComponent.vue.models';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import DataExportService from '@services/DataExportService';
import { HCDataExportStreamViewModel } from "@generated/Models/Module/DataExport/HCDataExportStreamViewModel";
import PagingComponent from '@components/Common/Basic/PagingComponent.vue';
import HashUtils from '@util/HashUtils';
import { RouteLocationNormalized } from "vue-router";
import DateUtils from "@util/DateUtils";
import UrlUtils from "@util/UrlUtils";
import { HCGetDataExportStreamDefinitionsViewModel } from "@generated/Models/Module/DataExport/HCGetDataExportStreamDefinitionsViewModel";
import { HCDataExportQueryResponseViewModel } from "@generated/Models/Module/DataExport/HCDataExportQueryResponseViewModel";
import { HCDataExportStreamItemDefinitionViewModel } from "@generated/Models/Module/DataExport/HCDataExportStreamItemDefinitionViewModel";
import EditorComponent from "@components/Common/EditorComponent.vue";
import BackendInputComponent from "@components/Common/Inputs/BackendInputs/BackendInputComponent.vue";
import { HCDataExportStreamQueryPresetViewModel } from "@generated/Models/Module/DataExport/HCDataExportStreamQueryPresetViewModel";
import { HCDataExportQueryRequest } from "@generated/Models/Module/DataExport/HCDataExportQueryRequest";
import { HCDataExportExporterViewModel } from "@generated/Models/Module/DataExport/HCDataExportExporterViewModel";
import { HCDataExportValueFormatterConfig } from "@generated/Models/Module/DataExport/HCDataExportValueFormatterConfig";
import { HCDataExportValueFormatterViewModel } from "@generated/Models/Module/DataExport/HCDataExportValueFormatterViewModel";
import StringUtils from "@util/StringUtils";
import { StoreUtil } from "@util/StoreUtil";

@Options({
    components: {
        draggable,
        FilterableListComponent,
        PagingComponent,
        EditorComponent,
        BackendInputComponent
    }
})
export default class DataExportPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<any>;
    
    // Service
    service: DataExportService = new DataExportService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);
    dataLoadStatus: FetchStatus = new FetchStatus();
    metadataLoadStatus: FetchStatus = new FetchStatus();
    exportLoadStatus: FetchStatus = new FetchStatus();
    deleteLoadStatus: FetchStatus = new FetchStatus();

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
    headers: Array<string> = [];
    headerNameOverrides: { [key:string]: string } = {};
    customParameters: { [key:string]: string } = {};
	valueFormatterConfigs: { [key:string]: HCDataExportValueFormatterConfig } = {};
    itemDefinition: HCDataExportStreamItemDefinitionViewModel = { StreamId: '', Name: '', Members: [] };
    customColumns: { [key:string]: string } = {};
    
    newPresetName: string = '';
    loadPresetDialogVisible: boolean = false;
    savePresetDialogVisible: boolean = false;
    presets: Array<HCDataExportStreamQueryPresetViewModel> = [];
    presetToDelete: HCDataExportStreamQueryPresetViewModel | null = null;
    exportDialogVisible: boolean = false;
    columnConfigDialogVisible: boolean = false;
    queryHelpDialogVisible: boolean = false;
    deletePresetDialogVisible: boolean = false;
    selectedFormatterId: string | null = null;
    selectedFormatHeader: string | null = null;
    formattersInDialog: Array<HCDataExportValueFormatterViewModel> = [];
    formatDialogVisible: boolean = false;
    placeholdersDialogVisible: boolean = false;
    hasQueriedAtLeastOnce: boolean = false;
    currentPlaceholderDialogTarget: string | null = null;

    // Filter/pagination
    pageIndex: number = 0;
    pageSize: number = 50;
    queryInput: string = '';
    totalResultCount: number = 0;
    filterCache: any = {};

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    async mounted()
    {
        StoreUtil.store.commit('showMenuButton', true);

        this.resetFilter();
        this.loadStreamDefinitions();

        this.refreshEditorSize();
        this.$nextTick(() => this.refreshEditorSize());
        setTimeout(() => {
            this.refreshEditorSize();
        }, 100);

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

    get hasAccessToQuery(): boolean {
        return this.hasAccessToQueryCustom || this.hasAccessToQueryPreset;
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

    get isSimpleExportMode(): boolean {
        return this.options.AccessOptions.length == 2
            && this.hasAccessToLoadPreset
            && this.hasAccessToExport;
    }

    get showExecuteQuery(): boolean {
        return this.hasAccessToQueryCustom
            || (this.selectedPresetId != null && this.hasAccessToQueryPreset);
    }

    get showExport(): boolean {
        return this.hasAccessToExport 
            && (this.selectedPresetId != null || this.hasAccessToQueryCustom)
            && this.exporters.length > 0;
    }

    get showQuery(): boolean {
        if (this.isSimpleExportMode) return false;
        if (!this.selectedStream || !this.selectedStream.ShowQueryInput) return false;
        return this.hasAccessToQueryCustom || this.selectedPresetId != null;
    }

    get showCustomInputs(): boolean {
        if (this.isSimpleExportMode) return false;
        return !!this.selectedStream
            && this.selectedStream.CustomParameterDefinitions
            && this.selectedStream.CustomParameterDefinitions.length > 0
            && (this.hasAccessToQueryCustom || this.selectedPresetId != null);
    }

    get queryTitle(): string {
        const def = this.showQuery ? 'Query' : 'Filter';

        if (this.hasAccessToQueryCustom) return def;
        else if (!this.selectedPresetId) return def;

        const preset = this.presets.filter(x => x.Id == this.selectedPresetId)[0];
        if (!preset) return def;
        return preset.Name || def;
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

    get selectedFormatter(): HCDataExportValueFormatterViewModel | null {
        if (!this.selectedFormatterId || !this.selectedStream) return null;
        return this.selectedStream.ValueFormatters.filter(x => x.Id == this.selectedFormatterId)[0];
    }

    get formattersInDialogChoices(): Array<HCDataExportValueFormatterViewModel>
    {
        let items: Array<HCDataExportValueFormatterViewModel> = [
            {
                Id: <any>null,
                Name: 'No formatting',
                Description: '',
                SupportedTypes: [],
                CustomParameterDefinitions: []
            },
            ...this.formattersInDialog
        ];
        return items;
    }

    get hasAnyHeaderOverrides(): boolean {
        return this.headerNameOverrides
            && Object.keys(this.headerNameOverrides).filter(k => this.headerNameOverrides[k] && this.headerNameOverrides[k].length > 0).length > 0;
    }

    get visibleHeaderNameOverrides(): any {
        let obj: any = {};
        if (this.headerNameOverrides)
        {
            Object.keys(this.headerNameOverrides).forEach(x => {
                if (this.headerNameOverrides[x] && this.headerNameOverrides[x].length > 0)
                {
                    obj[x] = this.headerNameOverrides[x];
                }
            });
        }
        return obj;
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
        this.headers = [];
        this.headerNameOverrides = {};
        this.customParameters = {};
        this.valueFormatterConfigs = {};
        this.customColumns = {};
        this.selectedPresetId = null;
        this.selectedFormatHeader = null;
        this.selectedFormatterId = null;
    }

    loadStreamDefinitions(): void {
        this.service.GetStreamDefinitions(this.metadataLoadStatus, {
            onSuccess: (data) => this.onStreamDefinitionsRetrieved(data)
        });
    }

    onStreamDefinitionsRetrieved(data: HCGetDataExportStreamDefinitionsViewModel | null): void {
        this.streamDefinitions = data;

        const idFromHash = StringUtils.stringOrFirstOfArray(this.$route.params.streamId) || null;
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

        if (updateUrl && StringUtils.stringOrFirstOfArray(this.$route.params.streamId) != StringUtils.stringOrFirstOfArray(this.hash(stream.Id)))
        {
            this.$router.push(`/dataExport/${this.hash(stream.Id)}`);
        }

        // Show preset selection dialog if no preset selected and no access to custom query
        this.$nextTick(() => {
            if (!this.hasAccessToQueryCustom
                && this.hasAccessToQueryPreset
                && !this.loadPresetDialogVisible
                && this.selectedPresetId == null)
            {
                this.onLoadPresetsClicked();
            }
            else if (this.isSimpleExportMode)
            {
                this.loadPresets();
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
        this.customParameters = preset.CustomParameters || {};
        this.valueFormatterConfigs = preset.ValueFormatterConfigs || {};
        this.customColumns = preset.CustomColumns || {};
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
        this.headers.forEach(x => {
            if (this.includedProperties.includes(x))
            {
                this.includedProperties = this.includedProperties.filter(p => p != x);
                this.includedProperties.unshift(x);
            }
        });
        this.includedProperties = this.includedProperties.reverse();

        this.service.SaveStreamQueryPreset({
            StreamId: this.selectedStream?.Id || '',
            Preset: {
                Id: existingPreset?.Id || '00000000-0000-0000-0000-000000000000',
                Name: name,
                Description: '',
                Query: this.queryInput,
                IncludedProperties: this.headers,
                HeaderNameOverrides: this.headerNameOverrides,
                CustomParameters: this.customParameters,
                ValueFormatterConfigs: this.valueFormatterConfigs,
                CustomColumns: this.customColumns
            }
        }, this.dataLoadStatus,
        {
            onSuccess: (d) => {
                this.savePresetDialogVisible = false;
            }
        });
    }

    isCustomHeader(header: string): boolean {
        return Object.keys(this.customColumns).includes(header);
    }

    hasFormatterForHeader(header: string): boolean {
        return this.getFormattersForHeader(header).length > 0;
    }

    getFormattersForHeader(header: string): Array<HCDataExportValueFormatterViewModel> {
        if (!this.itemDefinition || !this.selectedStream || !this.selectedStream.ValueFormatters) return [];
        const member = this.itemDefinition.Members.filter(x => x.Name == header)[0];
        if (!member) return [];

        return member.FormatterIds
            .map(x => this.selectedStream?.ValueFormatters.filter(f => f.Id == x)[0] || null)
            .filter(x => x != null) as Array<HCDataExportValueFormatterViewModel>;
        // return this.getFormattersForType(member.TypeName);
    }

    getFormattersForType(type: string): Array<HCDataExportValueFormatterViewModel> {
        if (!this.selectedStream || !this.selectedStream.ValueFormatters) return [];
        let cleanType = type;
        if (cleanType.startsWith('Nullable<'))
        {
            cleanType = cleanType.substring('Nullable<'.length);
            cleanType = cleanType.substring(0, cleanType.length - 1);
        }
        return this.selectedStream.ValueFormatters
            .filter(x => x.SupportedTypes
                    && (x.SupportedTypes.includes(type) || x.SupportedTypes.includes(cleanType)));
    }

    loadCurrentStreamItems(resetPageIndex: boolean): void {
        if (!this.selectedStream) return;

        this.updateUrlFromFilter();
        if (resetPageIndex)
        {
            this.pageIndex = 0;
        }

        const payload = this.createQueryPayload();
        this.service.QueryStreamPaged(payload, this.dataLoadStatus, {
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

        // Update headers
        // - Custom columns
        Object.keys(this.customColumns).forEach(x => {
            if (!this.headers.includes(x))
            {
                this.headers.push(x);
            }
        });
        // - Selected properties
        this.includedProperties.forEach(x => {
            if (!this.headers.includes(x))
            {
                this.headers.push(x);
            }
        });
        this.headers = this.headers.filter(x => this.includedProperties.includes(x) || Object.keys(this.customColumns).includes(x));

        return {
            StreamId: this.selectedStream?.Id || '',
            PageIndex: this.pageIndex,
            PageSize: this.pageSize,
            Query: this.hasAccessToQueryCustom ? this.queryInput : '',
            IncludedProperties: this.hasAccessToQueryCustom ? this.headers : [],
            PresetId: this.hasAccessToQueryCustom ? undefined : (this.selectedPresetId || undefined),
            HeaderNameOverrides: this.hasAccessToQueryCustom ? this.headerNameOverrides : {},
            ExporterId: exporterId || '',
            CustomParameters: this.customParameters,
            ValueFormatterConfigs: this.valueFormatterConfigs,
            CustomColumns: this.customColumns
        };
    }

    onStreamItemsLoaded(data: HCDataExportQueryResponseViewModel): void {
        this.hasQueriedAtLeastOnce = true;
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
            headerOverrides: this.headerNameOverrides,
            customParameters: this.customParameters,
            valueFormatterConfigs: this.valueFormatterConfigs,
            customColumns: this.customColumns
        };
    }

    applyFilterFromCache(streamId: string): boolean {
        const cache = this.filterCache[streamId];
        if (cache == null) return false;
        
        this.queryInput = this.filterCache[streamId].q || '';
        this.selectedPresetId = this.filterCache[streamId].pId || null;
        this.includedProperties = this.filterCache[streamId].incProp || [];
        this.headers = this.filterCache[streamId].headers || [];
        this.headerNameOverrides = this.filterCache[streamId].headerOverrides || {};
        this.customParameters = this.filterCache[streamId].customParameters || {};
        this.valueFormatterConfigs = this.filterCache[streamId].valueFormatterConfigs || {};
        this.customColumns = this.filterCache[streamId].customColumns || {};

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

    onRouteChanged(to: RouteLocationNormalized, from: RouteLocationNormalized): void {
        if (!this.streamDefinitions) return;

        const oldStreamIdFromHash = StringUtils.stringOrFirstOfArray(from.params.streamId) || null;
        const newStreamIdFromHash = StringUtils.stringOrFirstOfArray(to.params.streamId) || null;
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
        this.columnConfigDialogVisible = true;
    }

    onQueryHelpClicked(): void {
        this.queryHelpDialogVisible = true;
    }

    onDeletePresetClicked(target: HCDataExportStreamQueryPresetViewModel): void {
        this.presetToDelete = target;
        this.deletePresetDialogVisible = true;
    }

    onDeletePresetConfirmed(): void {
        if (!this.presetToDelete)
        {
            return;
        }
        this.service.DeleteStreamQueryPreset({
            StreamId: this.selectedStream?.Id || '',
            Id: this.presetToDelete.Id
        }, this.deleteLoadStatus, {
            onSuccess: () => {
                this.deletePresetDialogVisible = false;
                this.presets = this.presets.filter(x => x.Id != this.presetToDelete?.Id);
                this.presetToDelete = null;
            }
        });
    }

    onValueFormatButtonClicked(header: string): void {
        this.formattersInDialog = this.getFormattersForHeader(header);

        this.selectedFormatHeader = header;
        this.selectedFormatterId = null;
        const config = this.valueFormatterConfigs[header];
        if (config)
        {
            this.selectedFormatterId = this.formattersInDialog.filter(x => x.Id == config.FormatterId)[0]?.Id
                || null;
        }

        this.formatDialogVisible = true;
    }

    onFormatterChanged(): void {
        const header = this.selectedFormatHeader;
        if (header)
        {
            if (this.selectedFormatter && this.selectedFormatterId)
            {
                let customParameters: { [key:string]: string } = {};
                this.selectedFormatter?.CustomParameterDefinitions.forEach(x => {
                    customParameters[x.Id] = x.DefaultValue
                });

                this.valueFormatterConfigs[header] = {
                    FormatterId: this.selectedFormatterId,
                    PropertyName: header,
                    CustomParameters: customParameters,
                    Parameters: {}
                };
            }
            // Selected no formatting
            else if (this.selectedFormatterId == null)
            {
                delete this.valueFormatterConfigs[header];
            }
        }
    }

    onAddCustomHeaderButtonClicked(): void {
        for(let i=1;i<=1000;i++)
        {
            const key = `#Custom_${i}`;
            if (this.customColumns[key] == undefined)
            {
                this.customColumns[key] = '';
                this.headers.push(key);
                return;
            }
        }

        alert('Rly? You have over 1000 columns? More than that I didn\t expect was needed and isn\'t supported yet.');
    }

    onRemoveCustomHeaderButtonClicked(header: string): void {
        delete this.customColumns[header];
        this.headers = this.headers.filter(x => x != header);
    }

    onShowPlaceholdersClicked(header: string): void {
        this.placeholdersDialogVisible = true;
        this.currentPlaceholderDialogTarget = header;
    }

    onAddPlaceholderClicked(placeholder: string): void {
        if (!this.currentPlaceholderDialogTarget)
        {
            return;
        }
        this.customColumns[this.currentPlaceholderDialogTarget] += `{${placeholder}}`;
        this.placeholdersDialogVisible = false;
    }
}
</script>

<style scoped lang="scss">
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
.export-parameter-items {
    display: flex;
    flex-wrap: wrap;
    justify-content: space-between;
    .export-parameter-item {
        min-width: 48%;
    }
}
.item-header-override-config {
    border-left: 4px solid #e7e7e7;
    margin-bottom: 10px;
    padding-left: 10px;

    &__row {
        display: flex;
        align-items: baseline;
    }
}
.exporter-button {
    height: auto;
    .exporter-button-content {
        display: flex;
        flex-direction: row;
        flex-wrap: nowrap;
        .exporter-button-icon {
            align-self: center;
        }
        .exporter-button-texts {
            text-align: left;
            padding: 5px;

            .exporter-description {
                text-transform: none;
                font-size: 12px;
                white-space: break-spaces;
            }
        }
    }
}
.simple-export-buttons {
    display: flex;
    flex-direction: column;
    align-content: flex-start;
    align-items: flex-start;
}
</style>
