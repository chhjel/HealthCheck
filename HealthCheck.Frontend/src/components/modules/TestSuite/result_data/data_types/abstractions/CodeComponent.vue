<!-- src/components/modules/TestSuite/result_data/data_types/abstractions/CodeComponent.vue -->
<template>
    <div>
        <editor-component
            class="editor"
            :class="`${(fullscreen ? 'fullscreen' : '')}`"
            v-model="data.Data"
            :read-only="true"
            :language="language"
            ref="editor"/>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';
import EditorComponent from '@components/Common/EditorComponent.vue';

@Options({
    components: {
        EditorComponent
    }
})
export default class CodeComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;
    @Prop({ required: true })
    language!: string;

    mounted(): void {
    }

    refreshSize(): void {
        const editor: EditorComponent = <EditorComponent>this.$refs.editor;
        editor.refreshSize();
    }
    
    ////////////////////////////////////////////////////////////
    //// EVENTHANDLERS /////////////////////////////////////////
    ////////////////////////////////////////////////////////////
    @Watch("fullscreen")
    onFullscreenChanged(): void {
        this.refreshSize();
        this.$nextTick(() => this.refreshSize());
        setTimeout(() => {
            this.refreshSize();
        }, 100);
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