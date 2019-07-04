<!-- src/components/result_data/data_types/TestResultPlainTextDataComponent.vue -->
<template>
    <div>
        <v-textarea
          :label="data.Title"
          :value="data.Data"
          readonly
          :rows="rowCount"
          class="data-textarea"
        ></v-textarea>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestResultDataDumpViewModel from "../../../../models/TestResultDataDumpViewModel";

@Component({
    components: {
    }
})
export default class TestResultPlainTextDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;

    mounted(): void {
    }

    get hasTitle(): boolean {
      return this.data.Title != null && this.data.Title.length > 0;
    }

    get rowCount(): number {
      let lineCount = this.data.Data.split(/\r\n|\r|\n/).length;
      return Math.min(10, lineCount);
    }
}
</script>

<style>
</style>
