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
                    :groupByKey="`GroupName`"
                    :sortByKey="`GroupName`"
                    :iconsKey="'Icons'"
                    :filterKeys="[ 'Name' ]"
                    ref="filterableList"
                    v-on:itemClicked="onMenuItemClicked"
                    />
                    
                <div class="pl-4 pt-2">
                    <v-btn flat dark
                        color="#62b5e4"
                        @click="onNewScriptClicked"
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
                    :readOnly="loadStatus.inProgress"
                    ref="editor"
                    ></editor-component>

                <div class="middle-toolbar">
                    <v-btn flat dark
                        color="#62b5e4"
                        @click="onSaveLocalClicked"
                        >Save local</v-btn>

                    <v-btn flat dark
                        color="#62b5e4"
                        @click="onSaveToServerClicked"
                        >Save to server</v-btn>

                    <v-btn flat dark
                        color="#62b5e4"
                        @click="onDeleteClicked"
                        >Delete</v-btn>
                    
                    <v-btn flat dark
                        color="#62b5e4"
                        class="right"
                        @click="onExecuteClicked"
                        :loading="loadStatus.inProgress"
                        >Execute</v-btn>
                </div>

                <editor-component
                    class="codeeditor codeeditor__output"
                    language="json"
                    v-model="resultData"
                    ></editor-component>

            </v-container>
            </v-flex>
            </v-layout>
            </v-container>
        </v-content>
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

interface DynamicCodeExecutionPageOptions {
    InitialCode: string;
}

interface ServerSideScript {
    script: DynamicCodeScript;
}

interface LocalOnlyScript {
    script: DynamicCodeScript;
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

    localScripts: Array<LocalOnlyScript> = [];
    serverScripts: Array<ServerSideScript> = [];
    
    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();
    drawerState: boolean = true;

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.$store.commit('showMenuButton', true);
        this.code = this.initialCode;
        this.loadData();
    }

    created(): void {
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

    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
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
                    subtitle: x.script.Description,
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
            : `namespace CodeTesting 
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
}`;
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
            onSuccess: (d) => this.serverScripts = d.map(x => {
                const serverScript: ServerSideScript = {
                    script: x
                };
                return serverScript;
            })
        });
    }

    writeLocalScriptsToLocalStorage(scripts: Array<LocalOnlyScript>): void {
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

        setTimeout(() => {
            this.editor.foldRegions();
        }, 100);
    }

    openNewScript(): void {
        this.localScripts.push({
            script: {
                Id: IdUtils.generateId(),
                Title: this.getNewScriptName(),
                Description: '',
                Code: this.initialCode
            }
        });
        this.writeLocalScriptsToLocalStorage(this.localScripts);
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

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
    onNewScriptClicked(): void {
        this.openNewScript();
    }

    onSaveLocalClicked(): void {
        if (this.currentScript == null)
        {
            return;
        }

        this.currentScript.Code = this.code;
        let currentId = this.currentScript.Id;
        let localScript = this.localScripts.filter(x => x.script.Id == currentId)[0];
        localScript.script = this.currentScript;
        
        this.writeLocalScriptsToLocalStorage(this.localScripts);
    }

    onSaveToServerClicked(): void {
        if (this.currentScript == null)
        {
            return;
        }
    }

    onDeleteClicked(): void {
        if (this.currentScript == null)
        {
            return;
        }

        let currentId = this.currentScript.Id;
        this.localScripts = this.localScripts.filter(x => x.script.Id != currentId);
        this.writeLocalScriptsToLocalStorage(this.localScripts);

        this.currentScript = null;
    }

    onMenuItemClicked(item: any): void {
        const script = item.data.Script as DynamicCodeScript;
        this.openScript(script)
    }

    onEditorInit(editor: any): void {
        this.editor.foldRegions();
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
            
            // if (this.optionAutoformatDumps) {
            //     setTimeout(() => {
            //         const editor = (<any>this.$refs["dumpsEditor"])["editor"];
            //         editor.getAction('editor.action.formatDocument').run();
            //     }, 10);
            // }
            // if (this.optionAutoFoldRegions) {
            //     setTimeout(() => {
            //          this.codeEditor.runEditorAction('editor.foldAllMarkerRegions');
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
            //           max   toolbar  output  other
            height: calc(100vh - 50px - 30vh - 107px);
        }

        &__output {
            height: 30vh;
        }
    }
}
</style>