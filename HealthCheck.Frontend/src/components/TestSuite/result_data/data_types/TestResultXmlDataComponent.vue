<!-- src/components/result_data/data_types/TestResultXmlDataComponent.vue -->
<template>
    <div>
      <div v-if="!fullscreen">{{data.Title}}</div>
      <highlight-code 
        :class="`${(fullscreen ? 'code-field fullscreen' : 'code-field')}`"
        lang="xml"
        :code="data.Data"></highlight-code>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestResultDataDumpViewModel from "../../../../models/TestSuite/TestResultDataDumpViewModel";

@Component({
    components: {
    }
})
export default class TestResultXmlDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;

    mounted(): void {
    }
}
</script>

<style scoped>
.code-field:not(.fullscreen) {
    max-height: 300px;
    overflow: auto;
}
</style>
