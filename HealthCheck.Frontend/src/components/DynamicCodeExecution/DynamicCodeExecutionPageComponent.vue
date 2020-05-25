<!-- src/components/DynamicCodeExecution/DynamicCodeExecutionPageComponent.vue -->
<template>
    <div class="dce_page">
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
                    :sortByKey="`Name`"
                    :groupByKey="`GroupName`"
                    :iconsKey="'Icons'"
                    :filterKeys="[ 'Name' ]"
                    :disabled="loadStatus.inProgress"
                    ref="filterableList"
                    v-on:itemClicked="onMenuItemClicked"
                    />
                    
                <div class="pl-4 pt-2">
                    <v-btn flat dark
                        color="#62b5e4"
                        @click="onNewScriptClicked"
                        :disabled="!allowCreateNewScript"
                        ><v-icon>add</v-icon>New script</v-btn>
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
                    v-model="code"
                    v-on:editorInit="onEditorInit"
                    :readOnly="loadStatus.inProgress || currentScript == null"
                    ref="editor"
                    ></editor-component>

                <div class="middle-toolbar">
                    <v-btn flat dark
                        color="#62b5e4"
                        :disabled="!hasUnsavedChanges"
                        @click="onSaveClicked"
                        >Save</v-btn>

                    <v-btn flat dark
                        color="#62b5e4"
                        @click="onDeleteClicked"
                        :disabled="currentScript == null || loadStatus.inProgress || !canDeleteCurrentScript"
                        >Delete</v-btn>
                    
                    <v-btn flat dark
                        color="#62b5e4"
                        class="right"
                        @click="onExecuteClicked"
                        :loading="loadStatus.inProgress"
                        :disabled="currentScript == null || loadStatus.inProgress"
                        >Execute</v-btn>
                </div>

                <editor-component
                    ref="outputEditor"
                    class="codeeditor codeeditor__output"
                    language="json"
                    v-model="resultData"
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
            max-width="350"
            content-class="confirm-dialog">
            <v-card>
                <v-card-title class="headline">Confirm deletion</v-card-title>
                <v-card-text>
                    {{ deleteScriptDialogText }}
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary" @click="deleteScriptDialogVisible = false">Cancel</v-btn>
                    <v-btn color="error" @click="deleteScript(currentScript)">Delete it</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <!-- ##################### -->
        <v-dialog v-model="saveScriptDialogVisible"
            max-width="400"
            content-class="confirm-dialog">
            <v-card>
                <v-card-title class="headline">Save new script</v-card-title>
                <v-card-text>
                    Choose where to save this script.
                </v-card-text>
                <v-divider></v-divider>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="secondary" @click="saveScriptDialogVisible = false">Cancel</v-btn>
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
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import FrontEndOptionsViewModel from '../../models/Common/FrontEndOptionsViewModel';
import DateUtils from "../../util/DateUtils";
import LinqUtils from "../../util/LinqUtils";
import KeyArray from "../../util/models/KeyArray";
import KeyValuePair from "../../models/Common/KeyValuePair";
import BlockComponent from '../../components/Common/Basic/BlockComponent.vue';
import EventNotificationService from "../../services/EventNotificationService";
import ModuleConfig from "../../models/Common/ModuleConfig";
import ModuleOptions from "../../models/Common/ModuleOptions";
import EditorComponent from "../Common/EditorComponent.vue";
import { FetchStatus } from "../../services/abstractions/HCServiceBase";
import DynamicCodeExecutionService from "../../services/DynamicCodeExecutionService";
import { DynamicCodeExecutionResultModel, DynamicCodeScript } from "../../models/DynamicCodeExecution/Models";
import { MarkerSeverity } from "monaco-editor";
import { FilterableListItem } from "../Common/FilterableListComponent.vue";
import FilterableListComponent from '.././Common/FilterableListComponent.vue';
import IdUtils from "../../util/IdUtils";
import * as monaco from 'monaco-editor'

interface DynamicCodeExecutionPageOptions {
    InitialCode: string;
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
}

@Component({
    components: {
        BlockComponent,
        EditorComponent,
        FilterableListComponent
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

    serverScripts: Array<ServerSideScript> = [];
    localScripts: Array<LocalOnlyScript> = [];
    
    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();
    drawerState: boolean = true;
    deleteScriptDialogVisible: boolean = false;
    saveScriptDialogVisible: boolean = false;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.$store.commit('showMenuButton', true);
        this.loadData();

        window.addEventListener("beforeunload", (e) => this.onWindowUnload(e));

        this.openNewScript();
    }

    created(): void {
        // ToDo: add new parent event to check if module switching is allowed /w confirmation message
        this.$parent.$parent.$on("onSideMenuToggleButtonClicked", this.toggleSideMenu);
    }

    beforeDestroy(): void {
      this.$parent.$parent.$off('onSideMenuToggleButtonClicked', this.toggleSideMenu);
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

    get showEditor(): boolean {
        return this.currentScript != null;
    }

    get menu(): FilterableListComponent {
        return this.$refs.filterableList as FilterableListComponent;
    }

    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }

    get allowCreateNewScript(): boolean {
        return this.currentScript == null || this.currentScript.IsDraft != true;
    }

    get hasUnsavedChanges(): boolean {
        return this.currentScript != null 
            && (this.currentScript.Code != this.code || this.currentScript.IsDraft == true);
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
                        Icons: ['cross']
                    }
                }
            });
    }
    
    get initialCode(): string {
        return (this.options.Options != null
                && this.options.Options.InitialCode != null 
                && this.options.Options.InitialCode.length > 0)
            ? this.options.Options.InitialCode
            : `// Title: 
#region Usings
#endregion

namespace CodeTesting 
{
    public class EntryClass
    {
        public void Main() 
        {
            new { Hello = "World" }.Dump();
        }

        #region Docs
        // To dump a value use the extension methods .Dump():
        // * T Dump<T>(this T obj, string title = null, bool display = true, bool ignoreErrors = true)
        // * T Dump<T>(this T obj, string title = null, bool display = true, params JsonConverter[] converters)
        // * T Dump<T>(this T obj, Func<T, string> dumpConverter, string title = null, bool display = true)

        // To save all dumped values to a file:
        // * DCEUtils.SaveDumps(string pathOrFilename = null, bool includeTitle = false, bool includeType = false)
        //   Default filename is %tempfolder%\\DCEDump_{date}.txt
        #endregion
    }
}
`;
    }

    ////////////////
    //  METHODS  //
    //////////////
    toggleSideMenu(): void {
        this.drawerState = !this.drawerState;
    }

    loadData(): void {
        this.localScripts = this.getLocalScriptsFromLocalStorage();

        this.service.GetScripts(this.loadStatus, {
            onSuccess: (d) => {
                this.serverScripts = d.map(x => {
                    const serverScript: ServerSideScript = {
                        script: x
                    };
                    return serverScript;
                });
            }
        });
    }

    updateLocalStorage(scripts: Array<LocalOnlyScript>): void {
        const json = JSON.stringify(scripts);
        localStorage.setItem('localScripts', json);
    }

    getLocalScriptsFromLocalStorage(): Array<LocalOnlyScript>
    {
        const localScriptsJson = localStorage.getItem('localScripts');
        if (localScriptsJson != null)
        {
            return JSON.parse(localScriptsJson);
        }
        return [];
    }

    openScript(script: DynamicCodeScript): void {
        this.currentScript = script;
        this.code = this.currentScript.Code;

        this.$nextTick(() => this.menu.setSelectedItemByFilter(x => {
            return x.data.Script.Id == script.Id;
        }));

        setTimeout(() => {
            this.editor.foldRegions();
        }, 100);
    }

    openNewScript(): void {
        this.openScript(this.generateDraftScript());
    }

    generateDraftScript(): DynamicCodeScript {
        const name = this.getNewScriptName();
        let code = this.initialCode.replace(
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

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onEditorInit(editor: monaco.editor.IStandaloneCodeEditor): void {
        this.editor.foldRegions();

        editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KEY_S, () => {            
            this.onSaveClicked();
        });
    }

    onWindowUnload(e: any): string | undefined {
        if (!this.hasUnsavedChanges) {
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
                this.openScript(script);
            });
        }
        else if (location == 'server'
            || (location == null && this.scriptIsServerSide(script)))
        {
            let existOnServer = this.serverScripts.some(x => x.script.Id == script.Id);

            // Create new
            if (!existOnServer)
            {
                this.service.AddNewScript(script, this.loadStatus, {
                    onSuccess: (updatedScript) => {
                        this.serverScripts.push({
                            script: updatedScript
                        });
                        
                        this.$nextTick(() => {
                            this.openScript(script);
                        });
                    }
                });
            }
            // Update existing
            else
            {
                this.service.SaveScriptChanges(script, this.loadStatus, {
                    onSuccess: (updatedScript) => {
                        script.Code = updatedScript.Code;

                        this.$nextTick(() => {
                            this.openScript(script);
                        });
                    }
                });
            }
        }
        else {
            console.warn(`Save action did nothing.`);
            console.warn({ script: script, location: location });
        }
    }

    preprocessBeforeSave(script: DynamicCodeScript): void
    {
        Vue.set(script, 'IsDraft', false);
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

        if (this.currentScript.IsDraft)
        {
            this.saveScriptDialogVisible = true;
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
        const script = item.data.Script as DynamicCodeScript;
        this.openScript(script)
    }

    onExecuteClicked(): void {
        this.service.ExecuteCode({
            Code: this.code,
            DisabledPreProcessorIds: []
        }, this.loadStatus, {
            onSuccess: (d) => this.onCodeExecuted(d)
        });
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
            if (result.CodeExecutionResult.Code != null)
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
            
            // if (this.optionAutoformatDumps) {
            //     setTimeout(() => {
            //         this.outputEditor.editor.getAction('editor.action.formatDocument').run();
            //     }, 10);
            // }
            // if (this.optionAutoFoldRegions) {
            //     setTimeout(() => {
            //          this.editor.editor.runEditorAction('editor.foldAllMarkerRegions');
            //     }, 10);
            // }
        }

        this.resultData = resultText;
    }
}
</script>

<style scoped lang="scss">
.dce_page {
    background-color: hsla(0, 0%, 16%, 1);
    height: 100%;

    .middle-toolbar {
        height: 50px;
    }

    .codeeditor {
        box-shadow: 0 2px 4px 1px rgba(0, 0, 0, 0.15), 0 3px 2px 0 rgba(0,0,0,.02), 0 1px 2px 0 rgba(0,0,0,.06);

        &__input {
            position: relative;
            //           max   toolbar  output  other
            height: calc(100vh - 50px - 30vh - 107px);
        }

        &__output {
            height: 30vh;
        }
    }
}
</style>