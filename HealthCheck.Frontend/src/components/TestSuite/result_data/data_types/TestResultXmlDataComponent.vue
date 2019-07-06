<!-- src/components/result_data/data_types/TestResultXmlDataComponent.vue -->
<template>
    <div>
      <div>{{data.Title}}</div>
      <highlight-code lang="xml" :code="data.Data"></highlight-code>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestResultDataDumpViewModel from "../../../../models/TestResultDataDumpViewModel";

@Component({
    components: {
    }
})
export default class TestResultXmlDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;

    mounted(): void {
    }
}
</script>

<style>
</style>
