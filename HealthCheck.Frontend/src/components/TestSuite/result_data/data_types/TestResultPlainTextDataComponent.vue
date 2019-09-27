<!-- src/components/result_data/data_types/TestResultPlainTextDataComponent.vue -->
<template>
    <div>
        <v-textarea
          :label="data.Title"
          :value="text"
          readonly
          :rows="rowCount"
          class="data-textarea"
          :autoGrow="fullscreen"
        ></v-textarea>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import TestResultDataDumpViewModel from "../../../../models/TestSuite/TestResultDataDumpViewModel";

@Component({
    components: {
    }
})
export default class TestResultPlainTextDataComponent extends Vue {
    @Prop({ required: true })
    data!: TestResultDataDumpViewModel;
    @Prop({ required: true })
    fullscreen!: boolean;

    extraText: string = "";

    mounted(): void {
    }

    get text(): string {
      return this.data.Data + this.extraText;
    }

    get rowCount(): number {
      let lineCount = this.data.Data.split(/\r\n|\r|\n/).length;
      return Math.min(10, lineCount);
    }

    @Watch("fullscreen")
    onFullscreenChanged(): void {
      if (this.fullscreen == true) {
        this.extraText = " ";
        this.$nextTick(() => this.extraText = "");
      }
    }
}
</script>

<style scoped>
</style>
