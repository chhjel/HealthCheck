<!-- src/components/modules/TestSuite/result_data/data_types/TestResultCodeDataComponent.vue -->
<template>
    <div>
      <code-editor language="csharp"
        :data="data"
        :fullscreen="fullscreen" />
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';
import { CodeComponent } from '@components/modules/TestSuite/result_data/data_types/abstractions/CodeComponent.vue'

@Options({
    components: {
        CodeEditor
    }
})
export default class TestResultCodeDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;
}
</script>