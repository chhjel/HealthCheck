<!-- src/components/modules/TestSuite/result_data/data_types/TestResultJsonDataComponent.vue -->
<template>
    <div>
      <code-editor language="json"
        :data="data"
        :fullscreen="fullscreen" />
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestResultDataDumpViewModel from  '../../../../../models/modules/TestSuite/TestResultDataDumpViewModel';
import CodeEditor from './abstractions/CodeComponent.vue'

@Component({
    components: {
        CodeEditor
    }
})
export default class TestResultJsonDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;
}
</script>