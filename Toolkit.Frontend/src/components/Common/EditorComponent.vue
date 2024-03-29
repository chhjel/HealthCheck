<!-- src/components/Common/EditorComponent.vue -->
<template>
    <div class="editor-component" :class="rootClasses">
        <div class="editor-component__loader-bar" v-if="!isEditorInited">
            <progress-linear-component 
                indeterminate
                width="4"
                color="primary"
            />
        </div>

        <div ref="editorElement"
            class="editor-component__editor"
            :class="{ 'editor-component__editor__fullscreen': (isFullscreen) }"
            ></div>

        <btn-component absolute dark icon flat top right
            color="success"
            class="editor-fullscreen-button"
            title="Fullscreen"
            v-if="allowFullscreen"
            @click.stop="isFullscreen = true">
            <icon-component>fullscreen</icon-component>
        </btn-component>
        
        <!-- ##################### -->
        <!-- DIALOG TOOLBAR -->
        <div v-if="isFullscreen" class="editor-toolbar flex">
            <div class="spacer">{{ title }}</div>
            <div class="editor-toolbar__close" @click="isFullscreen = false">Close</div>
        </div>
        <!-- ##################### -->
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
// or import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';
// if shipping only a subset of the features & languages is desired
import * as monaco from 'monaco-editor'
import FrontEndOptionsViewModel from '@models/Common/FrontEndOptionsViewModel';
import { ICodeMark, CodeSnippet } from '@models/modules/DynamicCodeExecution/Models';
import { StoreUtil } from "@util/StoreUtil";


@Options({
    components: {
    }
})
export default class EditorComponent extends Vue {
    @Prop({ required: false, default: "json" })
    language!: string;

    @Prop({ required: false, default: false })
    readOnly!: boolean;

    @Prop({ required: false, default: "vs-dark" }) // 'vs' (default), 'vs-dark', 'tk-black'
    theme!: string;

    @Prop({ required: false, default: "" })
    value!: string;

    @Prop({ required: false, default: false })
    allowFullscreen!: boolean;

    @Prop({ required: false, default: "" })
    title!: string;

    @Prop({ required: false, default: () => []})
    suggestions!: Array<CodeSnippet>;

    @Prop({ required: false, default: false })
    includeBuiltInSuggestions!: boolean;

    // UI state
    isFullscreen: boolean = false;

    isEditorInited: boolean = false;
    currentDecorations: string[] = [];

    editor!: monaco.editor.IStandaloneCodeEditor;

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

    public goFullscreen(): void {
        this.isFullscreen = true;
    }

    public refreshSize(): void {
        if (this.editor) {
            this.editor.layout();
        }
    }

    public markCode(marks: ICodeMark[]): void {
        const model = this.editor.getModel();
        if (model == null) return;

        monaco.editor.setModelMarkers(model, "owner",
            marks.map(x => { return {
                startLineNumber: x.line,
                endLineNumber: x.line,
                startColumn: x.startColumn,
                endColumn: x.endColumn,
                message: x.message,
                severity: x.severity
            }})
        );
    }

    public foldRegions(): void {
        this.editor.getAction("editor.foldAllMarkerRegions").run();
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
        return StoreUtil.store.state.globalOptions;
    }

    get rootClasses(): any {
        let classes: any = {
            'editor-readonly': this.readOnly
        };
        return classes;
    }
    
    ////////////////////////////////////////////////////////////
    //// EVENTHANDLERS /////////////////////////////////////////
    ////////////////////////////////////////////////////////////
    isNull: boolean = false;
    @Watch("value")
    onValueChanged(): void
    {
        this.isNull = (this.value == null);

        if (this.editor == null) return;
        
        const model = this.editor.getModel();
        if (model == null) return;
        else if (model.getValue() == this.value) return;

        model.setValue(this.value || '');
    }

    @Watch("readOnly")
    onReadOnlyChanged(): void
    {
        if (this.editor == null) return;
        this.editor.updateOptions({
            readOnly: this.readOnly
        });
    }

    onWindowResize(): void {
        this.refreshSize();
    }
    
    onEditorInit(): void {
        this.listenForChanges();

        this.$emit('editorInit', this.editor);
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
                    case 'editorWorkerService': return this.processUrl(this.globalOptions.EditorConfig.EditorWorkerUrl);
                    case 'json': return this.processUrl(this.globalOptions.EditorConfig.JsonWorkerUrl);
                    case 'sql': return this.processUrl(this.globalOptions.EditorConfig.SqlWorkerUrl);
                    case 'html': return this.processUrl(this.globalOptions.EditorConfig.HtmlWorkerUrl);
                }
                return `/tk/unknown/monaco/worker/${label}.js`;
            }
        }
    }

    processUrl(url: string): string
    {
        if (url.startsWith('blob:')) {
            return this.createBlobUrlFor(url.substr(5));
        }
        return url;
    }

    createBlobUrlFor(url: string): string
    {
        const blob = new Blob(["importScripts('" + url + "');"], { type: 'application/javascript' });
        return URL.createObjectURL(blob);
    }

    createMonacoEditor(): monaco.editor.IStandaloneCodeEditor
    {
        const editor = monaco.editor.create(<HTMLElement>this.$refs.editorElement,
        {
            value: this.value,
            automaticLayout: true,
            language: this.language,
            readOnly: this.readOnly,
            glyphMargin: !this.readOnly,
            theme: this.theme
        });
        
        const fitHeightToContent: boolean = false;
        if (fitHeightToContent)
        {
            editor.onDidChangeModelDecorations(() => {
                this.fitEditorHeightToContent()
                requestAnimationFrame(this.fitEditorHeightToContent)
            });
        }

        this.checkForInit();
        return editor;
    }

    configureEditor(): void {
        this.registerBuiltInMethods();
        this.registerCustomSnippets();
    }

    registerBuiltInMethods(): void {
        monaco.languages.registerCompletionItemProvider(this.language, {
            // triggerCharacters: ['.'],
            provideCompletionItems: (model, position) => {
                if (!this.includeBuiltInSuggestions) {
                    return { suggestions: [] };
                }

                var textUntilPosition = model.getValueInRange({
                    startLineNumber: 1, startColumn: 1, endLineNumber: position.lineNumber, endColumn: position.column
                });

                var match = textUntilPosition.match(/\.$/);
                if (!match) {
                    return { suggestions: [] };
                }

                var word = model.getWordUntilPosition(position);
                var range = {
                    startLineNumber: position.lineNumber,
                    endLineNumber: position.lineNumber,
                    startColumn: word.startColumn,
                    endColumn: word.endColumn
                };
                return {
                    suggestions: this.createBuiltInSuggestions(model.id, range)
                };
            }
        });
    }

    createBuiltInSuggestions(modelId: string, range: monaco.IRange): Array<monaco.languages.CompletionItem> {
        if (this.editor.getModel() == null) return [];

        const model = this.editor.getModel();
        if (model == null || model.id != modelId) {
            return [];
        }

        return [
            {
                kind: monaco.languages.CompletionItemKind.Function,
                insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                range: range,
                label: '.Dump()',
                documentation: 'T Dump<T>(this T obj, string title = null, bool display = true, bool ignoreErrors = true)',
                insertText: 'Dump()'
            },
            {
                kind: monaco.languages.CompletionItemKind.Function,
                insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                range: range,
                label: '.Dump("title")',
                documentation: 'T Dump<T>(this T obj, string title = null, bool display = true, bool ignoreErrors = true)',
                insertText: 'Dump("${1:title}")'
            },
            {
                kind: monaco.languages.CompletionItemKind.Function,
                insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                range: range,
                label: '.Dump((obj) => obj.ToString())',
                documentation: 'T Dump<T>(this T obj, Func<T, string> dumpConverter, string title = null, bool display = true)',
                insertText: 'Dump((obj) => ${1:obj.ToString()})'
            }
        ];
    }

    registerCustomSnippets(): void {
        monaco.languages.registerCompletionItemProvider(this.language, {
            triggerCharacters: ['@@@.'],
            provideCompletionItems: (model, position) => {
                if (this.suggestions.length == 0) {
                    return { suggestions: [] };
                }

                var textUntilPosition = model.getValueInRange({
                    startLineNumber: 1, startColumn: 1, endLineNumber: position.lineNumber, endColumn: position.column
                });

                var match = textUntilPosition.match(/.*@@@\.$/);
                if (!match) {
                    return { suggestions: [] };
                }

                var word = model.getWordUntilPosition(position);
                var range = {
                    startLineNumber: position.lineNumber,
                    endLineNumber: position.lineNumber,
                    startColumn: word.startColumn - 4,
                    endColumn: word.endColumn
                };
                return {
                    suggestions: this.createSnippetSuggestions(model.id, range)
                };
            }
        });
    }

    createSnippetSuggestions(modelId: string, range: monaco.IRange): Array<monaco.languages.CompletionItem> {
        if (this.editor.getModel() == null) return [];

        const model = this.editor.getModel();
        if (model == null || model.id != modelId) {
            return [];
        }

        const suggestions = this.suggestions
            .map(x => {
                return {
                    kind: monaco.languages.CompletionItemKind.Function,
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    range: range,
                    label: x.label,
                    documentation: x.documentation,
                    insertText: x.insertText
                };
            });

        return suggestions;
    }

    checkForInit(): void {
        if (this.editor == undefined)
        {
            setTimeout(() => {
                this.checkForInit();
            }, 10);
            return;
        }

        this.isEditorInited = true;
        this.configureEditor();
        this.onEditorInit();
    }

    listenForChanges(): void {
        const model = this.editor.getModel();
        if (model == null) return;

        model.onDidChangeContent((e) => {
            let editorValue = model.getValue();
            if (this.isNull && !editorValue) editorValue = null;
            this.$emit('update:value', editorValue);
        })
    }

    // /**
    //  * Get an action that is a contribution to this editor.
    //  * @id Unique identifier of the contribution.
    //  * @return The action or null if action not found.
    //  */
    // getAction(id: string): IEditorAction;
    // /**
    //  * Execute a command on the editor.
    //  * The edits will land on the undo-redo stack, but no "undo stop" will be pushed.
    //  * @param source The source of the call.
    //  * @param command The command to execute
    //  */
    // executeCommand(source: string, command: ICommand): void;
    
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

<style scoped lang="scss">
.editor-component {
    position: relative;

    .editor-component__loader-bar {
        margin-bottom: -14px;
    }

    .editor-component__editor {
        width: 100%;
        height: 100%;
        
        &__fullscreen {
            position: fixed;
            top: 64px;
            left: 0;
            right: 0;
            height: calc(100% - 64px);
            z-index: 205;
        }
    }
}
.editor-fullscreen-button {
    top: 5px !important;
    right: 21px !important;
    overflow: hidden;
}
.editor-toolbar {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    height: 64px;
    z-index: 100;
    padding: 5px;
    box-sizing: border-box;
    background-color: var(--color--primary-darken2);
    color: #fff;

    &__close {
        display: flex;
        align-items: center;
        height: 100%;
        padding: 10px 30px;
        cursor: pointer;
        transition: 0.2s all;
        background-color: var(--color--primary-base);
        &:hover {
            background-color: var(--color--primary-lighten1);
        }
    }
}
</style>