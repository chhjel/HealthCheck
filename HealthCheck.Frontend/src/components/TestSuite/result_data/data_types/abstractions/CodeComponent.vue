<!-- src/components/result_data/data_types/abstractions/CodeComponent.vue -->
<template>
    <div>
        <div class="code-editor-loader" v-if="!isEditorInited">
            <v-progress-circular
                class="mr-2"
                indeterminate
                color="primary"
            />
            Loading {{ language }}..
        </div>

        <monaco-editor
            class="editor"
            :class="`${(fullscreen ? 'fullscreen' : '')}`"
            v-model="data.Data"
            v-on:editorDidMount="onEditorDidMount"
            :theme="monacoTheme"
            :options="monacoOptions"
            :syncInput="true"
            :language="language"
            ref="editor"/>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import TestResultDataDumpViewModel from "../../../../../models/TestSuite/TestResultDataDumpViewModel";
//@ts-ignore
import MonacoEditor from 'vue-monaco-cdn'

@Component({
    components: {
        MonacoEditor
    }
})
export default class CodeEditor extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;
    @Prop({ required: true })
    language!: string;

    isEditorInited: boolean = false;
    editor!: any;
    monacoTheme: string = "vs-dark";
    monacoOptions: any = {
        readOnly: true
    };

    mounted(): void {
        window.addEventListener('resize', this.onWindowResize);
    }

    beforeDestroy(): void {
        window.removeEventListener('resize', this.onWindowResize)
    }

    refreshSize(): void {
        if (this.editor) {
            this.editor.layout();
        }
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

    @Watch("fullscreen")
    onFullscreenChanged(): void {
        this.refreshSize();
        this.$nextTick(() => this.refreshSize());
    }
}
</script>

<style scoped>
.editor {
  width: 100%;
  height: 300px;
}
.editor.fullscreen {
  width: 100%;
  height: calc(100vh - 64px);
}
.code-editor-loader {
    display: flex;
    align-items: center;
    justify-content: center;
}
</style>