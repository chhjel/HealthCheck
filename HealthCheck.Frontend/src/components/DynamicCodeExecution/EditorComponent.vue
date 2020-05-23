<!-- src/components/DynamicCodeExecution/EditorComponent.vue -->
<template>
    <div>
        <!-- <div class="code-editor-loader" v-if="!isEditorInited">
            <v-progress-circular
                class="mr-2"
                indeterminate
                color="primary"
            />
            Loading {{ language }}..
        </div> -->

        <div ref="editorElement" class="editor-component__editor"></div>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
// //@ts-ignore
// import MonacoEditor from 'vue-monaco-cdn'
import { ICodeError } from "../../../../../RuntimeCodeTest/RuntimeCodeTest.DevTest.Mvc/VueDev/src/models/ICodeError";

import * as monaco from 'monaco-editor'
import FrontEndOptionsViewModel from "../../models/Common/FrontEndOptionsViewModel";
// or import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
// if shipping only a subset of the features & languages is desired

@Component({
    components: {
        // MonacoEditor
    }
})
export default class EditorComponent extends Vue {
    // @Prop({ required: true })
    // fullscreen!: boolean;

    // @Prop({ required: true })
    // language!: string;

    @Prop({ required: false, default: false })
    readOnly!: boolean;

    code: string = "";
    isEditorInited: boolean = false;
    currentDecorations: string[] = [];

    editor!: monaco.editor.IStandaloneCodeEditor;
    monacoTheme: string = "vs-dark";

    mounted(): void {
        this.configureMonacoEnv();
        this.editor = this.createMonacoEditor();

        window.addEventListener('resize', this.onWindowResize);
        setTimeout(() => {
            this.refreshSize();
        }, 10);
    }

    beforeDestroy(): void {
        window.removeEventListener('resize', this.onWindowResize)
    }

    refreshSize(): void {
        if (this.editor) {
            this.editor.layout();
        }
    }

    markErrors(errors: ICodeError[]): void {
        // this.currentDecorations = this.editor.deltaDecorations(
        //     this.currentDecorations, 
        //     errors.filter(err => err.line > -1).map((err) => ({ 
        //         range: new monaco.Range(err.line, 1, err.line, 1), 
        //         options: {
        //             stickiness: monaco.editor.TrackedRangeStickiness.NeverGrowsWhenTypingAtEdges,
        //             isWholeLine: true,
        //             className: 'errorLineContent',
        //             glyphMarginClassName: 'errorLineMargin',
        //             glyphMarginHoverMessage: { value: err.message }
        //         }
        //     }))
        // );
    }

    getCursorPosition(): number {
        var model = this.editor.getModel();
        let pos = this.editor.getPosition();
        if (model == null || pos == null) return 0;
        
        return model.getOffsetAt({ lineNumber: pos.lineNumber, column: pos.column });
    }

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return this.$store.state.globalOptions;
    }
    
    ////////////////////////////////////////////////////////////
    //// EVENTHANDLERS /////////////////////////////////////////
    ////////////////////////////////////////////////////////////
    onEditorDidMount(editor: any): void {
        this.editor = (<any>this.$refs.editor)["editor"];
        this.editor.layout();
        this.editor.updateOptions( { glyphMargin: true });
        this.isEditorInited = true;
    }

    onWindowResize(): void {
        this.refreshSize();
    }
    
    //////////////////////////////////////////////////////
    //// MONACO CONFIG //////////////////////////////////
    ////////////////////////////////////////////////////
    configureMonacoEnv(): void {
        (<monaco.Environment>(<any>self).MonacoEnvironment) =
        {
            getWorkerUrl: (moduleId: string, label: string): string =>
            {
                switch (label)
                {
                    case 'editorWorkerService': return this.globalOptions.EditorConfig.EditorWorkerUrl;
                    case 'json': return this.globalOptions.EditorConfig.JsonWorkerUrl;
                }
                return `/hc/unknown/monaco/worker/${label}.js`;
            }
        }
    }

    createMonacoEditor(): monaco.editor.IStandaloneCodeEditor
    {
        const editor = monaco.editor.create(<HTMLElement>this.$refs.editorElement,
        {
            value: 'console.log("Hello, world")',
            language: 'csharp',
            automaticLayout: true,
            readOnly: this.readOnly
        });
        
        const fitHeightToContent: boolean = false;
        if (fitHeightToContent)
        {
            editor.onDidChangeModelDecorations(() => {
                this.fitEditorHeightToContent()
                requestAnimationFrame(this.fitEditorHeightToContent)
            });
        }

        return editor;
    }
    
    prevHeight: number = 0;

    fitEditorHeightToContent(): void
    {
        const editorElement = this.editor.getDomNode()

        if (!editorElement) {
            return
        }

        const lineHeight = this.editor.getOption(monaco.editor.EditorOption.lineHeight)
        const lineCount = this.editor.getModel()?.getLineCount() || 1
        const height = this.editor.getTopForLineNumber(lineCount + 1) + lineHeight

        if (this.prevHeight !== height) {
            this.prevHeight = height
            editorElement.style.height = `${height}px`
            this.editor.layout()
        }
    }
}
</script>

<style scoped>
.editor-component__editor {
  width: 100%;
  height: 100%;
  /* display: block; */
}
</style>