<!-- src/components/result_data/data_types/TestResultJsonDataComponent.vue -->
<template>
    <div>
      <div>{{data.Title}}</div>
      <highlight-code lang="json" :code="data.Data"></highlight-code>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestResultDataDumpViewModel from "../../../../models/TestResultDataDumpViewModel";

@Component({
    components: {
    }
})
export default class TestResultJsonDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;

    mounted(): void {
    }

    get rowCount(): number {
      let lineCount = this.data.Data.split(/\r\n|\r|\n/).length;
      return Math.min(10, lineCount);
    }
}
</script>

<style>
</style>
