<!-- src/components/modules/TestSuite/result_data/data_types/TestResultDiffDataComponent.vue -->
<template>
    <div>
        <diff-component
            class="editor"
            :class="`${(fullscreen ? 'fullscreen' : '')}`"
            :allowFullscreen="false"
            :originalContent="originalContent"
            :modifiedContent="modifiedContent"
            :readOnly="true"
            ref="editor"
            />
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';
import DiffComponent from '@components/Common/DiffComponent.vue'

@Options({
    components: {
        DiffComponent
    }
})
export default class TestResultDiffDataComponent extends Vue {
    @Prop({ required: true })
    resultData!: TestResultDataDumpViewModel;
    
    @Prop({ required: true })
    fullscreen!: boolean;

    originalContent!: string;
    modifiedContent!: string;

    created(): void {
        const data = JSON.parse(this.resultData.Data);
        this.originalContent = data.Original;
        this.modifiedContent = data.Modified;
    }

    refreshSize(): void {
        const editor: DiffComponent = <DiffComponent>this.$refs.editor;
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
</style>
