<!-- src/components/Common/DiffComponent.vue -->
<template>
    <div class="diff-component">
        <div class="diff-component__loader-bar" v-if="!isEditorInited">
            <progress-linear-component 
                indeterminate
                width="4"
                color="primary"
            />
        </div>

        <div ref="editorElement"
            class="diff-component__editor"
            :class="{ 'diff-component__editor__fullscreen': (isFullscreen) }"
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
            <div class="spacer" style="text-align: center; margin-left: 73px;">{{ resolvedTitle }}</div>
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
import { StoreUtil } from "@util/StoreUtil";


@Options({
    components: {
    }
})
export default class DiffComponent extends Vue {
    @Prop({ required: false, default: "" })
    originalName!: string;

    @Prop({ required: false, default: "" })
    modifiedName!: string;

    @Prop({ required: false, default: "" })
    originalContent!: string;

    @Prop({ required: false, default: "" })
    modifiedContent!: string;

    @Prop({ required: false, default: "json" })
    originalContentLanguage!: string;

    @Prop({ required: false, default: "json" })
    modifiedContentLanguage!: string;

    @Prop({ required: false, default: false })
    readOnly!: boolean;

    @Prop({ required: false, default: "vs-dark" }) // 'vs' (default), 'vs-dark', 'tk-black'
    theme!: string;

    @Prop({ required: false, default: "" })
    leftSide!: string;

    @Prop({ required: false, default: "" })
    rightSide!: string;

    @Prop({ required: false, default: false })
    allowFullscreen!: boolean;

    @Prop({ required: false, default: "" })
    title!: string;

    // UI state
    isFullscreen: boolean = false;

    isEditorInited: boolean = false;
    editor!: monaco.editor.IStandaloneDiffEditor;

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

    ////////////////
    //  GETTERS  //
    //////////////
    get globalOptions(): FrontEndOptionsViewModel {
        return StoreUtil.store.state.globalOptions;
    }

    get resolvedTitle(): string {
        if (this.title) return this.title;
        else if (this.originalName && this.modifiedName) return `\"${this.originalName}\" vs \"${this.modifiedName}\"`;
        return '';
    }
    
    ////////////////////////////////////////////////////////////
    //// EVENTHANDLERS /////////////////////////////////////////
    ////////////////////////////////////////////////////////////
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

    createMonacoEditor(): monaco.editor.IStandaloneDiffEditor
    {
        const editor = monaco.editor.createDiffEditor(<HTMLElement>this.$refs.editorElement,
        {
            automaticLayout: true,
            readOnly: this.readOnly,
            glyphMargin: !this.readOnly,
            theme: this.theme,

        });

        var originalModel = monaco.editor.createModel(this.originalContent, this.originalContentLanguage);
        var modifiedModel = monaco.editor.createModel(this.modifiedContent, this.modifiedContentLanguage);
        editor.setModel({
            original: originalModel,
            modified: modifiedModel
        })

        this.checkForInit();
        return editor;
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
        this.onEditorInit();
    }
}
</script>

<style scoped lang="scss">
.diff-component {
    position: relative;

    &__loader-bar {
        margin-bottom: -14px;
    }

    &__editor {
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
    right: 40px !important;
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