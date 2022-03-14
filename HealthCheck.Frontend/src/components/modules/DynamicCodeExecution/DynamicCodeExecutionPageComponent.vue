<!-- src/components/modules/DynamicCodeExecution/DynamicCodeExecutionPageComponent.vue -->
<template>
    <div class="dce_page">
        <loading-screen-component ref="loadingscreen" text="LOADING DCE..." />

        <v-content>
            <!-- NAVIGATION DRAWER -->
            <v-navigation-drawer
                v-model="drawerState"
                clipped fixed floating app
                mobile-break-point="1000"
                :dark="localOptions.darkTheme"
                class="menu testset-menu">

                <filterable-list-component 
                    :items="menuItems"
                    :sortByKey="`Name`"
                    :groupByKey="`GroupName`"
                    :iconsKey="'Icons'"
                    :hrefKey="'Href'"
                    :filterKeys="[ 'Name' ]"
                    :disabled="loadStatus.inProgress"
                    ref="filterableList"
                    v-on:itemClicked="onMenuItemClicked"
                    />
                    
                <div class="pl-1 pr-1 pt-2 pb-2 menu-bottom-nav">
                    <v-btn flat :dark="localOptions.darkTheme"
                        color="#62b5e4"
                        @click="configDialogVisible = true"
                        ><v-icon>settings</v-icon>Settings</v-btn>

                    <v-tooltip top v-if="showCreateNewScriptButton">
                        <template v-slot:activator="{ on }">
                            <span v-on="on">
                            <v-btn flat :dark="localOptions.darkTheme"
                                color="#62b5e4"
                                @click="onNewScriptClicked"
                                :disabled="loadStatus.inProgress || !allowCreateNewScript"
                                ><v-icon>add</v-icon>New script</v-btn>
                            </span>
                        </template>
                        <span>{{ createNewScriptButtonTooltip }}</span>
                    </v-tooltip>
                </div>
            </v-navigation-drawer>

            <!-- CONTENT -->
            <v-container fluid fill-height class="content-root pt-3 pb-0 pl-0 pr-0">
            <v-layout>
            <v-flex>
            <v-container class="pt-0 pb-1 pl-0 pr-0 wrapper-container">

                <!-- DATA LOAD ERROR -->
                <v-alert :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </v-alert>

                <editor-component
                    class="codeeditor codeeditor__input"
                    language="csharp"
                    :theme="editorTheme"
                    :allowFullscreen="true"
                    v-model="code"
                    v-on:editorInit="onEditorInit"
                    :readOnly="isEditorReadOnly"
                    :title="currentScriptTitle"
                    :suggestions="suggestions"
                    :includeBuiltInSuggestions="true"
                    ref="editor"
                    ></editor-component>

                <div class="middle-toolbar">
                    <v-btn flat :dark="localOptions.darkTheme"
                        color="#62b5e4"
                        :disabled="!hasUnsavedChanges || loadStatus.inProgress"
                        v-if="showSaveButton"
                        @click="onSaveClicked"
                        >Save</v-btn>

                    <v-btn flat :dark="localOptions.darkTheme"
                        color="#ff6768"
                        @click="onDeleteClicked"
                        v-if="showDeleteButton"
                        :disabled="currentScript == null || loadStatus.inProgress || !canDeleteCurrentScript"
                        >Delete</v-btn>
                    
                    <v-btn flat outline :dark="localOptions.darkTheme"
                        color="#62b5e4"
                        class="right"
                        @click="onExecuteClicked"
                        :loading="loadStatus.inProgress"
                        v-if="showExecuteButton"
                        :disabled="currentScript == null || loadStatus.inProgress"
                        >
                        <v-icon class="mr-2">code</v-icon>
                        Execute</v-btn>
                </div>

                <editor-component
                    ref="outputEditor"
                    class="codeeditor codeeditor__output"
                    language="json"
                    :theme="editorTheme"
                    :allowFullscreen="true"
                    v-model="resultData"
                    :title="'Output'"
                    :readOnly="loadStatus.inProgress || currentScript == null"
                    ></editor-component>

            </v-container>
            </v-flex>
            </v-layout>
            </v-container>
        </v-content>
        
        <!-- ##################### -->
        <!-- ###### DIALOGS ######-->
        <v-dialog v-model="deleteScriptDialogVisible"
            @keydown.esc="deleteScriptDialogVisible = false"
            max-width="350" dark
            content-class="dce-dialog">
            <v-card color="secondary" class="white--text">
                <v-card-title class="headline">Confirm deletion</v-card-title>
                <v-card-text>
                    {{ deleteScriptDialogText }}
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="primary" @click="deleteScriptDialogVisible = false">Cancel</v-btn>
                    <v-btn color="error" @click="deleteScript(currentScript)">Delete it</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <!-- ##################### -->
        <v-dialog v-model="confirmUnchangedDialogVisible"
            @keydown.esc="unsavedChangesDialogGoBack()"
            max-width="350" dark
            content-class="dce-dialog">
            <v-card color="secondary" class="white--text">
                <v-card-title class="headline">Unsaved changes</v-card-title>
                <v-card-text>
                    It seems you have some unsaved changes.
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="primary"
                        @click="unsavedChangesDialogGoBack()"
                        >Go back</v-btn>
                    <v-btn color="error"
                        @click="unsavedChangesDialogConfirmed()"
                        >Discard changes</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <!-- ##################### -->
        <v-dialog v-model="saveScriptDialogVisible"
            @keydown.esc="saveScriptDialogVisible = false"
            max-width="400" dark
            content-class="dce-dialog">
            <v-card color="secondary" class="white--text">
                <v-card-title class="headline">Save new script</v-card-title>
                <v-card-text>
                    Choose where to save this script.
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="primary" @click="saveScriptDialogVisible = false">Cancel</v-btn>
                    <v-btn color="primary" @click="saveScript(currentScript, 'local')"
                        :disabled="loadStatus.inProgress"
                        :loading="loadStatus.inProgress"
                        >Local storage</v-btn>
                    <v-btn color="primary" @click="saveScript(currentScript, 'server')"
                        :disabled="loadStatus.inProgress"
                        :loading="loadStatus.inProgress"
                        >Server</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <!-- ##################### -->
        <v-dialog v-model="configDialogVisible"
            @keydown.esc="configDialogVisible = false"
            max-width="600" dark
            content-class="dce-dialog">
            <v-card color="secondary" class="white--text">
                <v-card-title class="headline">Settings</v-card-title>
                <v-card-text class="pt-0">
                    <div class="dce-dialog--option">
                        <v-checkbox
                            v-model="localOptions.autoFormatResult"
                            @change="(v) => setLocalConfig(o => o.autoFormatResult = v)"
                            label="Auto-format results" style="display:block"></v-checkbox>
                        <div class="dce-dialog--option--description">
                            Automatically formats json in result after execution.
                        </div>
                    </div>

                    <div class="dce-dialog--option">
                        <v-checkbox
                            v-model="localOptions.autoFoldRegions"
                            @change="(v) => setLocalConfig(o => o.autoFoldRegions = v)"
                            label="Auto-fold regions" style="display:block"></v-checkbox>
                        <div class="dce-dialog--option--description">
                            Automatically folds any #regions in editor after execution.
                        </div>
                    </div>

                    <div class="dce-dialog--option">
                        <v-checkbox
                            v-model="localOptions.updateLocalCodeFromRemote"
                            @change="(v) => setLocalConfig(o => o.updateLocalCodeFromRemote = v)"
                            label="Update local code from pre-processed" style="display:block"></v-checkbox>
                        <div class="dce-dialog--option--description">
                            Updates local code with its pre-processed version after execution.
                        </div>
                    </div>
                    
                    
                    <h3 class="mt-4 mb-2">Code pre-processors</h3>
                    <div v-for="(prepro, ppindex) in options.Options.PreProcessors"
                        :key="`dce_options_preprocessor-${ppindex}`"
                        class="dce-dialog--option"
                        >
                        <v-checkbox
                            :label="prepro.Name"
                            class="mt-0"
                            :disabled="!prepro.CanBeDisabled"
                            :input-value="isPreProcessorEnabled(prepro)"
                            @change="(v) => onPreProcessorToggled(prepro, v)"
                            ></v-checkbox>
                        <div class="dce-dialog--option--description"
                            v-if="prepro.Description != null && prepro.Description.trim().length > 0">
                            {{ prepro.Description }}
                        </div>
                    </div>
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="primary" @click="configDialogVisible = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <!-- ##################### -->
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import DateUtils from '@util/DateUtils';
import LinqUtils from '@util/LinqUtils';
import KeyArray from '@util/models/KeyArray';
import KeyValuePair from '@models/Common/KeyValuePair';
import BlockComponent from '@components/Common/Basic/BlockComponent.vue';
import ModuleConfig from '@models/Common/ModuleConfig';
import ModuleOptions from '@models/Common/ModuleOptions';
import EditorComponent from '@components/Common/EditorComponent.vue';
import { FetchStatus } from '@services/abstractions/HCServiceBase';
import DynamicCodeExecutionService from '@services/DynamicCodeExecutionService';
import { DynamicCodeExecutionResultModel, DynamicCodeScript, AutoCompleteRequest, AutoCompleteData, CodeSnippet } from '@models/modules/DynamicCodeExecution/Models';
import { MarkerSeverity } from "monaco-editor";
import { FilterableListItem } from '@components/Common/FilterableListComponent.vue.models';
import FilterableListComponent from '@components/Common/FilterableListComponent.vue';
import IdUtils from '@util/IdUtils';
import * as monaco from 'monaco-editor'
import HealthCheckPageComponent from '@components/HealthCheckPageComponent.vue';
import LoadingScreenComponent from '@components/Common/LoadingScreenComponent.vue';
import StringUtils from "@util/StringUtils";
import { StoreUtil } from "@util/StoreUtil";

interface DynamicCodeExecutionPageOptions {
    DefaultScript: string | null;
    PreProcessors: Array<PreProcessorMetadata>;
    ServerSideScriptsEnabled: boolean;
    AutoCompleteEnabled: boolean;
    StaticSnippets: Array<BackendCodeSnippet>;
}
interface BackendCodeSnippet {
    Description: string;
    Name: string;
    Suggestion: string;
}

interface PreProcessorMetadata {
    Id: string;
    Name: string;
    Description: string;
    CanBeDisabled: boolean;
}

interface ServerSideScript {
    script: DynamicCodeScript;
}

interface LocalOnlyScript {
    script: DynamicCodeScript;
}

interface LocalOptions {
    autoFormatResult: boolean;
    autoFoldRegions: boolean;
    updateLocalCodeFromRemote: boolean;
    darkTheme: boolean;
    disabledPreProcessorIds: Array<string>;
}

@Options({
    components: {
        BlockComponent,
        EditorComponent,
        FilterableListComponent,
        LoadingScreenComponent
    }
})
export default class DynamicCodeExecutionPageComponent extends Vue {
    @Prop({ required: true })
    config!: ModuleConfig;
    
    @Prop({ required: true })
    options!: ModuleOptions<DynamicCodeExecutionPageOptions>;

    service: DynamicCodeExecutionService = new DynamicCodeExecutionService(this.globalOptions.InvokeModuleMethodEndpoint, this.globalOptions.InludeQueryStringInApiCalls, this.config.Id);

    code: string = '';
    resultData: string = '';
    currentScript: DynamicCodeScript | null = null;
    localOptions: LocalOptions = this.loadLocalConfig();

    serverScripts: Array<ServerSideScript> = [];
    localScripts: Array<LocalOnlyScript> = [];
    
    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();
    deleteScriptDialogVisible: boolean = false;
    saveScriptDialogVisible: boolean = false;
    confirmUnchangedDialogVisible: boolean = false;
    configDialogVisible: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        StoreUtil.store.commit('showMenuButton', true);
        this.loadData();

        window.addEventListener("beforeunload", (e) => this.onWindowUnload(e));

        this.openNewScript(false);
    }

    created(): void {
        (<any>this.$parent)?.$parent.$on("onNotAllowedModuleSwitch", this.onNotAllowedModuleSwitch);
    }

    beforeDestroy(): void {
      (<any>this.$parent)?.$parent.$off('onNotAllowedModuleSwitch', this.onNotAllowedModuleSwitch);
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get editor(): EditorComponent {
        return this.$refs.editor as EditorComponent;
    }

    get outputEditor(): EditorComponent {
        return this.$refs.outputEditor as EditorComponent;
    }

    get editorTheme(): 'vs' | 'vs-dark' {
        return (this.localOptions.darkTheme) ? 'vs-dark' : 'vs';
    }

    get suggestions(): Array<CodeSnippet> {
        const snippets = this.options.Options.StaticSnippets || [];
        return snippets.map(x => {
            return {
                label: `@@@.${x.Name}`,
                documentation: x.Description,
                insertText: x.Suggestion
            }
        });
    }

    get showEditor(): boolean {
        return this.currentScript != null;
    }

    get menu(): FilterableListComponent {
        return this.$refs.filterableList as FilterableListComponent;
    }

    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get currentScriptTitle(): string {
        if (this.currentScript == null || this.currentScript.IsDraft == true) return 'Script';
        else return this.currentScript.Title;
    }

    get allowCreateNewScript(): boolean {
        return this.currentScript == null || this.currentScript.IsDraft != true;
    }

    get createNewScriptButtonTooltip(): string {
        if (this.currentScript != null && this.currentScript.IsDraft == true) return 'You are already editing a new script.';
        else return 'Click to create a new script';
    }

    get hasUnsavedChanges(): boolean {
        return this.currentScript != null 
            && (!this.codeIsEqual(this.currentScript.Code, this.code) || this.currentScript.IsDraft == true);
    }

    get shouldNotifyUnsavedChanges(): boolean {
        return this.currentScript != null 
            && !this.codeIsEqual(this.currentScript.Code, this.code);
    }
    
    get canDeleteCurrentScript(): boolean {
        return this.canDeleteScript(this.currentScript);
    }

    get deleteScriptDialogText(): string {
        if (this.currentScript == null)
        {
            return `Should not be able to end up here, this script should not be deletable. What did you do?`;
        }
        else if (this.scriptIsLocal(this.currentScript))
        {
            return `Are you sure you want to delete the script '${this.currentScript.Title}' from local storage?`;
        }
        else
        {
            return `Are you sure you want to delete the script '${this.currentScript.Title}' from the server?`;
        }
    }

    get showSaveButton(): boolean {
        if (this.currentScript == null) return false;
        else if(this.currentScript.IsDraft == true) return true;
        else if(this.scriptIsLocal(this.currentScript)) return true;
        else return this.canEditExistingScriptOnServer;
    }

    get showDeleteButton(): boolean {
        if (this.currentScript == null) return false;
        else if(this.scriptIsLocal(this.currentScript)) return true;
        else return this.canDeleteExistingScriptOnServer;
    }

    get showExecuteButton(): boolean {
        if (this.currentScript == null) return false;
        else if (this.scriptIsLocal(this.currentScript) && !this.canExecuteCustomScript) return false;
        else return this.canExecuteCustomScript || this.canExecuteSavedScript;
    }

    get showCreateNewScriptButton(): boolean {
        return this.canExecuteCustomScript || this.canCreateNewScriptOnServer;
    }

    get isEditorReadOnly(): boolean {
        if (this.loadStatus.inProgress || this.currentScript == null) return true;
        // else if (!this.canExecuteCustomScript) return true;
        else return false;
    }
    
    get menuItems(): Array<FilterableListItem>
    {
        const localItems = this.localScripts.map(x => {
            return {
                group: 'Local Scripts',
                script: x.script
            };
        });
        const serverItems = this.serverScripts.map(x => {
            return {
                group: 'Server Scripts',
                script: x.script
            };
        });

        return serverItems
            .concat(localItems)
            .map(x => {
                return {
                    title: x.script.Title,
                    subtitle: '',
                    data: {
                        GroupName: x.group,
                        Name: x.script.Title,
                        Script: x.script,
                        Icons: this.scriptIsServerSide(x.script) ? ['cloud'] : [],
                        Href: `#${this.config.InitialRoute}/${x.script.Id}`
                    }
                }
            });
    }
    
    get defaultScript(): string {
        return (this.options.Options != null
                && this.options.Options.DefaultScript != null 
                && this.options.Options.DefaultScript.length > 0)
            ? this.options.Options.DefaultScript
            : `// Title: 
namespace CodeTesting 
{
    public class EntryClass
    {
        public void Main() 
        {
            new { Hello = "World" }.Dump();
        }
    }
}
`;
    }

    // Options
    get canExecuteCustomScript(): boolean {
        return this.hasAccess('ExecuteCustomScript');
    }
    get canExecuteSavedScript(): boolean {
        return this.hasAccess('ExecuteSavedScript') && this.options.Options.ServerSideScriptsEnabled == true;
    }
    get canLoadScriptFromServer(): boolean {
        return this.hasAccess('LoadScriptFromServer') && this.options.Options.ServerSideScriptsEnabled == true;
    }
    get canCreateNewScriptOnServer(): boolean {
        return this.hasAccess('CreateNewScriptOnServer') && this.options.Options.ServerSideScriptsEnabled == true;
    }
    get canEditExistingScriptOnServer(): boolean {
        return this.hasAccess('EditExistingScriptOnServer') && this.options.Options.ServerSideScriptsEnabled == true;
    }
    get canDeleteExistingScriptOnServer(): boolean {
        return this.hasAccess('DeleteExistingScriptOnServer') && this.options.Options.ServerSideScriptsEnabled == true;
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch("shouldNotifyUnsavedChanges")
    onShouldNotifyUnsavedChangesCanged(): void {
        StoreUtil.store.commit('allowModuleSwitch', !this.shouldNotifyUnsavedChanges);
    }

    ////////////////////
    //  Parent Menu  //
    //////////////////
    drawerState: boolean = this.storeMenuState;
    get storeMenuState(): boolean {
        return StoreUtil.store.state.ui.menuExpanded;
    }
    @Watch("storeMenuState")
    onStoreMenuStateChanged(): void {
        this.drawerState = this.storeMenuState;
    }
    @Watch("drawerState")
    onDrawerStateChanged(): void {
        StoreUtil.store.commit('setMenuExpanded', this.drawerState);
    }

    ////////////////
    //  METHODS  //
    //////////////
    updateUrl(): void {
        let routeParams: any = {};
        if (this.currentScript != null && this.currentScript.IsDraft != true)
        {
            routeParams['id'] = this.currentScript.Id;
        }
        this.$router.push({ name: this.config.Id, params: routeParams })
    }

    updateSelectionFromUrl(): void {
        const selectedScriptId = StringUtils.stringOrFirstOfArray(this.$route.params.id) || '';

        if (selectedScriptId !== undefined && selectedScriptId.length > 0) {
            let script = this.localScripts.concat(this.serverScripts)
                .filter(x => x.script.Id == selectedScriptId)[0];
            if (script != null)
            {
                this.openScript(script.script, false);
            }
        }
    }

    loadData(): void {
        this.localScripts = this.getLocalScriptsFromLocalStorage();

        if (this.canLoadScriptFromServer)
        {
            this.service.GetScripts(this.loadStatus, {
                onSuccess: (d) => {
                    this.serverScripts = d.map(x => {
                        const serverScript: ServerSideScript = {
                            script: x
                        };
                        return serverScript;
                    });
                    this.updateSelectionFromUrl();
                }
            });
        }
        else
        {
            this.updateSelectionFromUrl();
        }
    }

    hasAccess(option: string): boolean {
        return this.options.AccessOptions.indexOf(option) != -1;
    }

    setLocalConfig(action: (o:LocalOptions) => void): void {
        action(this.localOptions);
        this.saveLocalConfig();
    }

    saveLocalConfig(): void {
        const json = JSON.stringify(this.localOptions);
        localStorage.setItem('__HC_DCE_localOptions', json);
    }

    loadLocalConfig(): LocalOptions {
        const json = localStorage.getItem('__HC_DCE_localOptions');
        if (json != null) {
            try {
                return JSON.parse(json);
            } catch(e)
            {
                console.warn('Failed to load local config, using defaults.');
                console.warn(e);
            }
        }
        
        return {
            autoFormatResult: false,
            autoFoldRegions: true,
            updateLocalCodeFromRemote: true,
            darkTheme: true,
            disabledPreProcessorIds: []
        };
    }

    updateLocalStorage(scripts: Array<LocalOnlyScript>): void {
        const json = JSON.stringify(scripts);
        localStorage.setItem('__HC_DCE_localScripts', json);
    }

    codeIsEqual(left: string, right: string): boolean
    {
        if (left == null && right == null) return true;
        else if (left == null || right == null) return false;
        else return (left.replace(/\r/g, "") == right.replace(/\r/g, ""));
    }

    confirmUnsavedChangesPromiseResolve!: any;
    confirmUnsavedChangesPromiseReject!: any;
    confirmUnsavedChanges(): Promise<boolean>
    {
        this.confirmUnchangedDialogVisible = true;
        return new Promise((resolve, reject) =>  {
            this.confirmUnsavedChangesPromiseResolve = resolve;
            this.confirmUnsavedChangesPromiseReject = reject;
        });
    }
    
    unsavedChangesDialogConfirmed(): void {
        this.confirmUnchangedDialogVisible = false;
        this.confirmUnsavedChangesPromiseResolve(true);
    }
    
    unsavedChangesDialogGoBack(): void {
        this.confirmUnchangedDialogVisible = false;
        this.confirmUnsavedChangesPromiseResolve(false);
    }

    getLocalScriptsFromLocalStorage(): Array<LocalOnlyScript>
    {
        const localScriptsJson = localStorage.getItem('__HC_DCE_localScripts');
        if (localScriptsJson != null)
        {
            return JSON.parse(localScriptsJson);
        }
        return [];
    }

    setSelectedMenuItem(script: DynamicCodeScript): void {
        this.$nextTick(() => this.menu.setSelectedItemByFilter(x => {
            return x.data.Script.Id == script.Id;
        }));
    }

    openScript(script: DynamicCodeScript, updateUrl: boolean = true): void {
        this.currentScript = script;
        this.code = this.currentScript.Code;

        this.setSelectedMenuItem(script);

        setTimeout(() => {
            this.editor.foldRegions();
        }, 100);
        
        if (updateUrl)
        {
            const idInUrl = StringUtils.stringOrFirstOfArray(this.$route.params.id);
            if (idInUrl !== script.Id)
            {
                this.updateUrl();
            }
        }
    }

    openNewScript(updateUrl: boolean = true): void {
        this.openScript(this.generateDraftScript(), updateUrl);
    }

    generateDraftScript(): DynamicCodeScript {
        const name = this.getNewScriptName();
        let code = this.defaultScript.replace(
            '// Title: New Script',
            `// Title: ${name}`
        );
        const script: DynamicCodeScript = {
                Id: IdUtils.generateId(),
                Title: name,
                Code: code,
                IsDraft: true
            };
        return script;
    }

    getNewScriptName(): string
    {
        let num = 1;
        this.localScripts.forEach(localScript => {
            const title = localScript.script.Title;
            if (title.startsWith('New Script '))
            {
                const numMatch = title.match(/.*?([0-9]+)$/);
                if (numMatch != null)
                {
                    num = parseInt(numMatch[1]) + 1;
                }
            }
        });

        return `New Script ${num}`;
    }

    openSuitableOrCreateDraft(): void {
        const suitableItem = this.menuItems
            .filter(x => x.data.Script != null)
            .sort((a, b) => LinqUtils.SortBy(a, b, x => x.title))[0];

        if (suitableItem != null) {
            this.openScript(suitableItem.data.Script);
        } else {
            this.openNewScript();
        }
    }

    scriptIsLocal(script: DynamicCodeScript): boolean {
        return this.localScripts.some(x => x.script.Id == script.Id);
    }

    scriptIsServerSide(script: DynamicCodeScript): boolean {
        return this.serverScripts.some(x => x.script.Id == script.Id);
    }

    canDeleteScript(script: DynamicCodeScript | null): boolean {
        return script != null && script.IsDraft !== true;
    }
    
    deleteScript(script: DynamicCodeScript): void {
        this.deleteScriptDialogVisible = false;

        if (!this.canDeleteScript(script))
        {
            return;
        }

        let scriptId = script.Id;

        // Remove matches from local scripts
        if (this.localScripts.some(x => x.script.Id == scriptId))
        {
            this.localScripts = this.localScripts.filter(x => x.script.Id != scriptId);
            this.updateLocalStorage(this.localScripts);
        }

        // Remove matches from server scripts
        if (this.serverScripts.some(x => x.script.Id == scriptId))
        {
            this.service.DeleteScript(scriptId, this.loadStatus, {
                onSuccess: (d) => {
                    this.serverScripts = this.serverScripts.filter(x => x.script.Id != scriptId);
                }
            });
        }

        if (this.currentScript != null && this.currentScript.Id == scriptId)
        {
            this.currentScript = null;
            this.code = '';
            this.$nextTick(() => this.openSuitableOrCreateDraft());
        }
    }

    isPreProcessorEnabled(p: PreProcessorMetadata): boolean {
        return !p.CanBeDisabled
            || this.localOptions.disabledPreProcessorIds.indexOf(p.Id) == -1;
    }

    onPreProcessorToggled(p: PreProcessorMetadata, enable: boolean): void {
        if (enable)
        {
            this.localOptions.disabledPreProcessorIds = this.localOptions.disabledPreProcessorIds.filter(x => x != p.Id)
        }
        else
        {
            this.localOptions.disabledPreProcessorIds.push(p.Id);
        }

        this.saveLocalConfig();
    }
    
    updateAutoCompleteProvider(): void {
        if (this.options.Options.AutoCompleteEnabled != true) {
            return;
        }

        let self = this;
        monaco.languages.registerCompletionItemProvider('csharp', {
            triggerCharacters: ['.'],
            provideCompletionItems: (model, position) => {
                return self.createAutoCompleteSuggestions();
            }
        });
    }

    createAutoCompleteSuggestions(): monaco.languages.ProviderResult<monaco.languages.CompletionList> {
        let payload: AutoCompleteRequest = {
            Code: this.code,
            Position: this.getCursorPosition()
        };
        
        const promise = new Promise<monaco.languages.CompletionList>((resolve, reject) => {
            this.service.AutoComplete(payload, null, {
                onSuccess: (d) => {
                    resolve({
                        suggestions: d.map(x => {
                            const item = {
                                kind: x.Kind,
                                label: x.Label,
                                insertText: x.InsertText,
                                documentation: x.Documentation
                            }
                            return item as monaco.languages.CompletionItem;
                        })
                    })
                },
                onError: (m) => reject()
            });
        });
        return promise.then();
    }

    getCursorPosition(): number {
        let pos = this.editor.editor.getPosition() || { lineNumber: 0, column: 0 };
        let model = this.editor.editor.getModel();
        if (model == null) return 0;
        return model.getOffsetAt({ lineNumber: pos.lineNumber, column: pos.column });
    }

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onEditorInit(editor: monaco.editor.IStandaloneCodeEditor): void {
        this.editor.foldRegions();

        editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, () => {            
            this.onSaveClicked();
        });

        (this.$refs.loadingscreen as LoadingScreenComponent).hide();
        this.updateAutoCompleteProvider();
    }

    onWindowUnload(e: any): string | undefined {
        if (!this.shouldNotifyUnsavedChanges) {
            return undefined;
        }

        var confirmationMessage = 'It looks like you have some unsaved changes. '
                                + 'If you leave before saving, your changes will be lost.';
        (e || window.event).returnValue = confirmationMessage; //Gecko + IE
        return confirmationMessage; //Gecko + Webkit, Safari, Chrome etc.
    }

    onNewScriptClicked(): void {
        this.openNewScript();
    }

    saveScript(script: DynamicCodeScript, location: 'local' | 'server' | null = null): void {
        this.saveScriptDialogVisible = false;

        script.Code = this.code;
        this.preprocessBeforeSave(script);
        this.code = script.Code;

        if (location == 'local' 
            || (location == null && this.scriptIsLocal(script)))
        {
            let localScript = this.localScripts.filter(x => x.script.Id == script.Id)[0];
            if (localScript == null)
            {
                localScript = { script: script };
                this.localScripts.push(localScript);
            }
            localScript.script = script;

            this.updateLocalStorage(this.localScripts);
                        
            this.$nextTick(() => {
                this.openScript(script, false);
            });
        }
        else if (location == 'server'
            || (location == null && this.scriptIsServerSide(script)))
        {
            let existOnServer = this.serverScripts.some(x => x.script.Id == script.Id);

            // Create new
            if (!existOnServer)
            {
                if (this.canCreateNewScriptOnServer)
                {
                    this.service.AddNewScript(script, this.loadStatus, {
                        onSuccess: (updatedScript) => {
                            this.serverScripts.push({
                                script: updatedScript
                            });
                            
                            this.$nextTick(() => {
                                this.openScript(script, false);
                            });
                        }
                    });
                }
                else { console.warn('You do not have access to create new scripts on the server.'); }
            }
            // Update existing
            else
            {
                if (this.canEditExistingScriptOnServer)
                {
                    this.service.SaveScriptChanges(script, this.loadStatus, {
                        onSuccess: (updatedScript) => {
                            script.Code = updatedScript.Code;

                            this.$nextTick(() => {
                                this.openScript(script, false);
                            });
                        }
                    });
                }
                else { console.warn('You do not have access to edit existing scripts on the server.'); }
            }
        }
        else {
            console.warn(`Save action did nothing.`);
            console.warn({ script: script, location: location });
            console.warn(`scriptIsServerSide: ${this.scriptIsServerSide(script)}`);
            console.warn(this.serverScripts);
        }
    }

    preprocessBeforeSave(script: DynamicCodeScript): void
    {
        script['IsDraft'] = false;
        delete script.IsDraft;

        const titleFromCode = this.getTitleFromComment(script.Code);
        if (titleFromCode == null || titleFromCode.length == 0)
        {
            const name = this.getNewScriptName();
            script.Title = name;
            script.Code = this.setCodeTitle(script.Code, name);
        }
        else
        {
            script.Title = titleFromCode;
        }
    }

    setCodeTitle(code: string, title: string): string
    {
        const titleFromCode = this.getTitleFromComment(code);
        if (titleFromCode == null)
        {
            code = `// Title: ${title}\n${code}`;
        }
        else
        {
            code = code.replace(this.titleRegex, `// Title: ${title}`);
        }
        return code;
    }

    titleRegex = /^\s*\/\/\s*Title:(.+)$/mi;
    getTitleFromComment(code: string): string | null
    {
        const match = code.match(this.titleRegex);
        if (match == null) return null;
        else return match[1].trim();
    }

    // Both Save-button and Ctrl-S in editor
    onSaveClicked(): void {
        if (this.currentScript == null || !this.hasUnsavedChanges)
        {
            return;
        }
        else if (this.scriptIsServerSide(this.currentScript) && !this.canEditExistingScriptOnServer)
        {
            return;
        }

        if (this.currentScript.IsDraft)
        {
            if (this.canCreateNewScriptOnServer)
            {
                this.saveScriptDialogVisible = true;
            }
            else
            {
                this.saveScript(this.currentScript, 'local');
            }
        }
        else
        {
            this.saveScript(this.currentScript);
        }
    }

    onDeleteClicked(): void {
        if (this.currentScript == null || !this.canDeleteCurrentScript)
        {
            return;
        }

        this.deleteScriptDialogVisible = true;
    }

    onMenuItemClicked(item: any): void {
        if (this.shouldNotifyUnsavedChanges)
        {
            if (this.currentScript != null)
            {
                this.setSelectedMenuItem(this.currentScript);
            }
            // setTimeout(() => this.setSelectedMenuItem(script), 50);

            this.confirmUnsavedChanges()
                .then(confirmed => {
                    if (confirmed)
                    {
                        this.openScript(item.data.Script as DynamicCodeScript)
                    }
                });
        }
        else
        {
            this.openScript(item.data.Script as DynamicCodeScript)
        }
    }

    onExecuteClicked(): void {
        const onSuccess: ((data: DynamicCodeExecutionResultModel) => void ) = (d) => {
            this.onCodeExecuted(d);
        };

        if (this.canExecuteCustomScript)
        {
            this.service.ExecuteCode({
                    Code: this.code,
                    DisabledPreProcessorIds: this.localOptions.disabledPreProcessorIds
                },
                this.loadStatus, { onSuccess: onSuccess });
        }
        else if (this.canExecuteSavedScript && this.currentScript != null)
        {
            this.service.ExecuteScriptById(this.currentScript.Id, this.loadStatus, { onSuccess: onSuccess });
        }
    }

    onCodeExecuted(result: DynamicCodeExecutionResultModel): void {
        let resultText = '';

        if (!result.Success)
        {
            resultText = `// ${result.Message}`;
        }
        // Success
        else if (result.CodeExecutionResult != null)
        {
            // Show errors
            if (result.CodeExecutionResult.Errors.length > 0)
            {
                resultText += `// ${result.CodeExecutionResult.Status}\n`;
                resultText += result.CodeExecutionResult.Errors
                    .map((x) => {
                        const header = (x.Line == -1) ? 'Error:' : `Line ${x.Line}`;
                        return `// ${header}\n${x.Message}\n`;
                    })
                    .join('\n');
            }

            // Include output if any
            if (result.CodeExecutionResult.Output != null)
            {
                resultText += `// Output: ${result.CodeExecutionResult.Output}\n\n`;
            }

            // Update code if pre-processed
            if (result.CodeExecutionResult.Code != null && this.localOptions.updateLocalCodeFromRemote == true)
            {
                this.code = result.CodeExecutionResult.Code;
            }

            // Show any dump output
            resultText += result.CodeExecutionResult.Dumps.map((x) => `// ${x.Title}\n${x.Data}\n`).join('\n') || '';

            // Mark errors
            this.editor.markCode(result.CodeExecutionResult.Errors.map(x => {
                return {
                    line: x.Line,
                    startColumn: x.Column,
                    endColumn: 999,
                    message: x.Message,
                    severity: MarkerSeverity.Error
                }
            }));

            // Go to next problem. This also clears the current problem popup if no problems exist.
            setTimeout(() => this.editor.editor.getAction('editor.action.marker.next').run(), 100);
            
            if (this.localOptions.autoFormatResult) {
                setTimeout(() => {
                    this.outputEditor.editor.getAction('editor.action.formatDocument').run();
                }, 10);
            }
            if (this.localOptions.autoFoldRegions) {
                setTimeout(() => {
                     this.editor.foldRegions();
                }, 10);
            }
        }

        this.resultData = resultText;
    }

    onNotAllowedModuleSwitch(): void {
        this.confirmUnsavedChanges()
            .then((confirmed) => {
                if (confirmed && this.currentScript != null)
                {
                    this.code = this.currentScript.Code;
                    setTimeout(() => ((<any>this.$parent)?.$parent as HealthCheckPageComponent).retryShowModule(), 50);
                }
            });
    }
}
</script>

<style scoped lang="scss">
.dce_page {
    background-color: hsla(0, 0%, 16%, 1);
    height: 100%;

    .middle-toolbar {
        height: 47px;
    }

    .codeeditor {
        box-shadow: 0 2px 4px 1px rgba(0, 0, 0, 0.15), 0 3px 2px 0 rgba(0,0,0,.02), 0 1px 2px 0 rgba(0,0,0,.06);

        &__input {
            position: relative;
            //           max   toolbar  output  other
            height: calc(100vh - 47px - 30vh - 109px);
        }

        &__output {
            height: 30vh;
        }
    }

    .menu {
        overflow: hidden;
    }

    .menu > div:first-of-type {
        height: calc(100% - 62px);
        overflow-y: auto;
    }

    .menu-bottom-nav {
        position: absolute;
        bottom: 0;
        left: 0;
        right: 0;
        height: 62px;
        overflow: hidden;
        display: flex;
        /* justify-content: flex-end; */
        box-shadow: 0px 0px 13px 0px #1b1b1b;

        button {
            flex: 1;
        }
    }

    /* .dce-config-dialog {
    } */
}
</style>

<style lang="scss">
.dce-dialog {
    .dce-dialog--option {
        margin-bottom: 10px;

        .v-input {
            margin-top: 0;

            .v-messages {
                display: none;
            }
        }

        .v-input__slot {
            margin-bottom: 0 !important;
        }
        
        .dce-dialog--option--description {
            font-size: small;
            margin-left: 32px;
        }
    }
}
</style>
