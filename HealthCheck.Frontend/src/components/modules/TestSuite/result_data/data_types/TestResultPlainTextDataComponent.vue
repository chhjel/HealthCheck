<!-- src/components/modules/TestSuite/result_data/data_types/TestResultPlainTextDataComponent.vue -->
<template>
    <div>
        <v-textarea
          :value="text"
          readonly
          :rows="rowCount"
          class="data-textarea mt-0"
          :autoGrow="fullscreen"
        ></v-textarea>
    </div>
</template>

<script lang="ts">
import { Vue, Prop, Watch } from "vue-property-decorator";
import { Options } from "vue-class-component";
import { TestResultDataDumpViewModel } from '@generated/Models/Core/TestResultDataDumpViewModel';

@Options({
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
