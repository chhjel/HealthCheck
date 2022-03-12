<!-- src/components/modules/TestSuite/result_data/data_types/TestResultXmlDataComponent.vue -->
<template>
    <div>
      <code-component language="xml"
        :resultData="resultData"
        :fullscreen="fullscreen" />
    </div>
</template>

<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';
import CodeComponent from '@components/modules/TestSuite/result_data/data_types/abstractions/CodeComponent.vue'

@Options({
    components: {
        CodeComponent
    }
})
export default class TestResultXmlDataComponent extends Vue {
    @Prop({ required: true })
    resultData!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;
}
</script>
