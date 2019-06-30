<!-- src/components/TestResultDataComponent.vue -->
<template>
    <div>
        <v-textarea
          :label="data.Title"
          :value="data.Data"
          readonly
          :rows="rowCount"
          class="data-textarea"
        ></v-textarea>

        <v-card-actions>
          <v-btn flat color="secondary-darken2">Copy</v-btn>
          <v-btn flat color="secondary-darken2">Fullscreen</v-btn>
        </v-card-actions>
    </div>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";
import TestViewModel from '../models/TestViewModel';
import TestResultDataDumpViewModel from "../models/TestResultDataDumpViewModel";

@Component({
    components: {
    }
})
export default class TestResultDataComponent extends Vue {
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
/* .data-textarea .v-input__slot {
  border: none;
  box-shadow: none !important;
} */
/* .data-textarea .v-input__slot::before {
  border: none !important;
} */
</style>
