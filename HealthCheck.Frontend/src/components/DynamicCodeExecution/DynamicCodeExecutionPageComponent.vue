<!-- src/components/DynamicCodeExecution/DynamicCodeExecutionPageComponent.vue -->
<template>
    <div>
        <v-content class="pl-0">
            <v-container fluid fill-height class="content-root">
            <v-layout>
            <v-flex>
            <v-container>

                <!-- DATA LOAD ERROR -->
                <v-alert :value="loadStatus.failed" v-if="loadStatus.failed" type="error">
                {{ loadStatus.errorMessage }}
                </v-alert>

                <!-- <code>{{ options }}</code>
                <code>{{ config }}</code> -->

                <editor-component style="height: 400px"
                    language="csharp"
                    v-model="code"
                    v-on:editorInit="onEditorInit"
                    ref="editor"
                    ></editor-component>

                <v-btn
                    @click="onExecuteClicked"
                    :loading="loadStatus.inProgress"
                    >Execute</v-btn>

                <editor-component style="height: 400px"
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
import { DynamicCodeExecutionResultModel } from "../../models/DynamicCodeExecution/Models";
import { MarkerSeverity } from "monaco-editor";

interface DynamicCodeExecutionPageOptions {
    InitialCode: string;
}

@Component({
    components: {
        BlockComponent,
        EditorComponent
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

    // UI STATE
    loadStatus: FetchStatus = new FetchStatus();

    //////////////////
    //  LIFECYCLE  //
    ////////////////
    mounted(): void
    {
        this.code = this.initialCode;
        // this.loadData();
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

    ///////////////////////
    //  EVENT HANDLERS  //
    /////////////////////
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
</style>